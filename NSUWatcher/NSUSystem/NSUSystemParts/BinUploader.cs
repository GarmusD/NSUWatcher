using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.NSUTypes;
using NSUWatcher.NSUWatcherNet;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class BinUploader : NSUSysPartsBase
    {
        private enum Stage
        {
            None,
            Upload,
            Write,
            Verify
        };

        private string BinFilePath { get { return Path.Combine(Config.Instance().NSUWritablePath, BinFile); } }

        private const string LogTag = "BinUploader";
        private const string BinFile = "nsu.bin";
        private FileStream binfile;
        private Process prc;
        private NetClientData client;
        private Stage stage = Stage.None;
        private Timer retryTimer;
        private int retryCount = 0;

        public BinUploader(NSUSys sys, PartsTypes type)
            : base(sys, type)
        {
            binfile = null;
            retryTimer = new Timer(3000);
            retryTimer.AutoReset = false;
            retryTimer.Elapsed += RetryTimer_Elapsed;
        }

        private void RetryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(stage == Stage.Write)
            {
                NSULog.Debug(LogTag, "Flash retry timer elapsed.");
                StartFlashProcess();
            }
        }

        public override string[] RegisterTargets()
        {
            return new string[] { JKeys.BinUploader.TargetName };
        }

        public override void Clear()
        {
            //
        }

        public override void ProccessArduinoData(JObject data)
        {
            //throw new NotImplementedException();
        }

        public override void ProccessNetworkData(NetClientData clientData, JObject data)
        {
            
            if(data.Property(JKeys.Generic.Action) != null)
            {
                string action = (string)data[JKeys.Generic.Action];
                switch (action)
                {
                    case JKeys.BinUploader.Abort:
                        if(stage == Stage.Upload)
                        {
                            try
                            {
                                CleanUp();
                            }
                            finally
                            {
                                stage = Stage.None;
                                binfile = null;
                                JObject jo = new JObject();
                                jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
                                jo[JKeys.Generic.Action] = JKeys.BinUploader.Abort;
                                jo[JKeys.Generic.Result] = JKeys.Result.Ok;
                                SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                            }
                        }
                        break;
                    case JKeys.BinUploader.StartUpload:
                        try
                        {
                            if (stage == Stage.None)
                            {
                                CleanUp();
                                binfile = new FileStream(BinFilePath, FileMode.Create, FileAccess.ReadWrite);
                                stage = Stage.Upload;
                                client = clientData;
                                JObject jo = new JObject();
                                jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
                                jo[JKeys.Generic.Action] = JKeys.BinUploader.StartUpload;
                                jo[JKeys.Generic.Result] = JKeys.Result.Ok;
                                SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                            }
                            else
                            {
                                JObject jo = new JObject();
                                jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
                                jo[JKeys.Generic.Action] = JKeys.BinUploader.StartUpload;
                                jo[JKeys.Generic.Result] = JKeys.Result.Error;
                                jo[JKeys.Generic.ErrCode] = JKeys.ErrCodes.BinUploader.FlashInProgress;
                                try
                                {
                                    NSULog.Debug(LogTag, "Another user is flashing???");
                                    NSULog.Debug(LogTag, $"Current flassher: {client?.ToString()}");
                                    NSULog.Debug(LogTag, $"New flasher: {clientData?.ToString()}");
                                }
                                finally { }
                                SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                            }
                        }
                        catch(Exception ex)
                        {
                            CleanUp();
                            JObject jo = new JObject();
                            jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
                            jo[JKeys.Generic.Action] = JKeys.BinUploader.StartUpload;
                            jo[JKeys.Generic.Result] = JKeys.Result.Error;
                            jo[JKeys.Generic.ErrCode] = JKeys.ErrCodes.BinUploader.FileCreateError;
                            jo[JKeys.Generic.Message] = ex.ToString();
                            NSULog.Exception(LogTag, "Action:Start - " + ex);
                            SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                        }
                        break;
                    case JKeys.BinUploader.Data:
                        try
                        {
                            if (stage == Stage.Upload)
                            {
                                var bindata = Convert.FromBase64String((string)data[JKeys.Generic.Value]);
                                binfile.Write(bindata, 0, bindata.Length);
                                JObject jo = new JObject();
                                jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
                                jo[JKeys.Generic.Action] = JKeys.BinUploader.Data;
                                jo[JKeys.Generic.Status] = (int)data[JKeys.Generic.Status];
                                jo[JKeys.Generic.Result] = JKeys.Result.Ok;
                                SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                            }
                            else
                            {
                                //Report error
                                NSULog.Debug(LogTag, "Not in progress");
                            }
                        }
                        catch (Exception ex)
                        {
                            CleanUp();
                            JObject jo = new JObject();
                            jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
                            jo[JKeys.Generic.Action] = JKeys.BinUploader.Data;
                            jo[JKeys.Generic.Result] = JKeys.Result.Error;
                            jo[JKeys.Generic.ErrCode] = JKeys.ErrCodes.BinUploader.FileCreateError;
                            jo[JKeys.Generic.Message] = ex.ToString();
                            NSULog.Exception(LogTag, "Action:Data - " + ex);
                            SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                        }                        
                        break;
                    case JKeys.BinUploader.DataDone:
                        if(stage == Stage.Upload)
                        {
                            try
                            {
                                binfile.Flush(true);
                                binfile.Position = 0;

                                //Compute hash
                                SHA256Managed sha = new SHA256Managed();
                                var hash = BitConverter.ToString(sha.ComputeHash(binfile)).Replace("-", string.Empty);

                                binfile.Dispose();
                                binfile = null;

                                JObject jo = new JObject();
                                jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
                                jo[JKeys.Generic.Action] = JKeys.BinUploader.DataDone;
                                jo[JKeys.Generic.Result] = JKeys.Result.Ok;
                                jo[JKeys.Generic.Value] = hash;
                                SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                            }
                            catch (Exception ex)
                            {
                                CleanUp();
                                JObject jo = new JObject();
                                jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
                                jo[JKeys.Generic.Action] = JKeys.BinUploader.DataDone;
                                jo[JKeys.Generic.Result] = JKeys.Result.Error;
                                jo[JKeys.Generic.ErrCode] = JKeys.ErrCodes.BinUploader.FileCreateError;
                                jo[JKeys.Generic.Message] = ex.ToString();
                                NSULog.Exception(LogTag, "Action:Data - " + ex);
                                SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                            }

                        }
                        break;
                    case JKeys.BinUploader.StartFlash:
                        if (stage == Stage.Upload)
                        {
                            StartFlashProcess();
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void CleanUp()
        {
            stage = Stage.None;
            try
            {
                if(binfile != null)
                {
                    binfile.Dispose();
                }
                if(File.Exists(BinFilePath))
                {
                    File.Delete(BinFilePath);
                }
            }
            finally
            {
                retryCount = 0;
                binfile = null;
            }

        }

        private void StartFlashProcess()
        {
            stage = Stage.Write;
            NSULog.Debug(LogTag, "StartFlashProcess()");
            nsusys.cmdCenter.Stop();
            //System.Threading.Thread.Sleep(300);
            ProcessStartInfo psi = new ProcessStartInfo(Config.Instance().Bossac);
            psi.Arguments = string.Format($"-i -d --port={Config.Instance().BossacPort} -U false -e -w -v -b {Path.Combine(Config.Instance().NSUWritablePath, BinFile)} -R");
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            NSULog.Debug(LogTag, $"Starting '{psi.FileName}' with arguments '{psi.Arguments}'");
            ReportInfo($"Starting: {psi.FileName}");
            ReportInfo($"Arguments: {psi.Arguments}");

            prc = new Process();
            prc.StartInfo = psi;
            prc.EnableRaisingEvents = true;
            prc.OutputDataReceived += HandlePrcOutputDataReceived;
            prc.Exited += HandlePrcExited;
            prc.Start();
            prc.BeginOutputReadLine();
        }

        private void HandlePrcOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string input = e.Data.Trim();
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                if (input.StartsWith("write")) return;
                if (input.StartsWith("read")) return;
                if (input.StartsWith("go")) return;

                NSULog.Debug(LogTag, "bossac: "+input);
                if (input.StartsWith("Write") && input.Contains("to flash"))
                {
                    stage = Stage.Write;
                    FlashStarted();
                }
                else if (input.StartsWith("Verify") && input.Contains("of flash"))
                {
                    stage = Stage.Verify;
                    VerifyStarted();
                }
                else if (input.StartsWith("["))
                {
                    ReportProgress(input);
                }
                else
                {
                    ReportInfo(input);
                }
            }
        }

        private void HandlePrcExited(object sender, EventArgs e)
        {
            NSULog.Debug(LogTag, "HandlePrcExited() exit code: "+prc.ExitCode.ToString());
            int exitCode = prc.ExitCode;
            int c = 0;
            while (!prc.HasExited && c < 15)
            {
                NSULog.Debug(LogTag, "!prc.HasExited: Sleeping...");
                System.Threading.Thread.Sleep(250);
                c++;
            }
            if (!prc.HasExited)
            {
                NSULog.Debug(LogTag, "Still !prc.HasExited: Killing...");
                prc.Kill();
            }

            NSULog.Debug(LogTag, "prc.Close();");
            prc.Close();
            prc = null;
            if (exitCode == 0)
            {                
                NSULog.Debug(LogTag, "Starting CmdCenter...");
                //System.Threading.Thread.Sleep(100);
                nsusys.cmdCenter.Start();
                ReportFinish();
            }
            else
            {
                //Report error
                JObject jo = new JObject();
                jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
                jo[JKeys.Generic.Action] = JKeys.BinUploader.StartFlash;
                jo[JKeys.Generic.Result] = JKeys.Result.Error;
                jo[JKeys.Generic.ErrCode] = JKeys.ErrCodes.BinUploader.BossacError;
                jo[JKeys.Generic.Value] = exitCode;
                NSULog.Exception(LogTag, "Bossac exit code - " + exitCode.ToString());
                SendToClient(NetClientRequirements.CreateStandartClientOnly(client), jo);

                //Send info about retry
                ReportInfo("Retrying in 3 seconds...");
                //Init repeat
                if(retryCount < 3)
                {
                    retryTimer.Enabled = true;
                }
            }
        }

        private void FlashStarted()
        {
            stage = Stage.Write;
            NSULog.Debug(LogTag, "Flash Started....");
            JObject jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
            jo[JKeys.Generic.Action] = JKeys.BinUploader.FlashStarted;
            SendToClient(NetClientRequirements.CreateStandartClientOnly(client), jo);
        }

        private void VerifyStarted()
        {
            stage = Stage.Verify;
            JObject jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
            jo[JKeys.Generic.Action] = JKeys.BinUploader.VerifyStarted;
            SendToClient(NetClientRequirements.CreateStandartClientOnly(client), jo);
        }

        private void ReportProgress(string input)
        {
            var result = input.Split(' ')
                .Where(s => s.Contains('%'))
                .Select(s => s.Trim('%')).First();
            //NSULog.Debug(LogTag, "ReportProgress() Searching percentage: " + result);
            int res = 0;
            if(int.TryParse(result, out res))
            {
                JObject jo = new JObject();
                jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
                jo[JKeys.Generic.Action] = JKeys.BinUploader.Progress;
                jo[JKeys.Generic.Value] = res;
                SendToClient(NetClientRequirements.CreateStandartClientOnly(client), jo);
            }
        }

        private void ReportInfo(string input)
        {
            JObject jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
            jo[JKeys.Generic.Action] = JKeys.BinUploader.InfoText;
            jo[JKeys.Generic.Value] = input;
            SendToClient(NetClientRequirements.CreateStandartClientOnly(client), jo);
            
        }

        private void ReportFinish()
        {
            stage = Stage.None;
            JObject jo = new JObject();
            jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
            jo[JKeys.Generic.Action] = JKeys.BinUploader.FlashDone;
            SendToClient(NetClientRequirements.CreateStandartClientOnly(client), jo);
        }
    }
}
