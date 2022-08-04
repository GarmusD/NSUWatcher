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

        private string BinFilePath => Path.Combine(_nsuSys.Config.NSUWritablePath, BinFile);

        private const string LogTag = "BinUploader";
        private const string BinFile = "nsu.bin";

        private readonly NSUSys _nsuSys;
        private FileStream _binFile;
        private Process _process;
        private NetClientData _clientData;
        private Stage _stage = Stage.None;
        private readonly Timer _retryTimer;
        private int _retryCount = 0;

        public BinUploader(NSUSys sys, PartsTypes type)
            : base(sys, type)
        {
            _nsuSys = sys;
            _binFile = null;
            _retryTimer = new Timer(3000)
            {
                AutoReset = false
            };
            _retryTimer.Elapsed += RetryTimer_Elapsed;
        }

        private void RetryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_stage == Stage.Write)
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
            if (data.Property(JKeys.Generic.Action) != null)
            {
                string action = (string)data[JKeys.Generic.Action];
                try
                {
                    switch (action)
                    {
                        case JKeys.BinUploader.Abort:
                            ActionAbort(clientData);
                            break;

                        case JKeys.BinUploader.StartUpload:
                            ActionStartUpload(clientData);
                            break;

                        case JKeys.BinUploader.Data:
                            ActionData(clientData, data);
                            break;

                        case JKeys.BinUploader.DataDone:
                            ActionDataDone(clientData);
                            break;
                        case JKeys.BinUploader.StartFlash:
                            ActionStartFlash();
                            break;
                        default:
                            break;
                    }
                }
                catch(Exception ex)
                {
                    CleanUp();
                    JObject jo = new JObject
                    {
                        [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                        [JKeys.Generic.Action] = JKeys.BinUploader.DataDone,
                        [JKeys.Generic.Result] = JKeys.Result.Error,
                        [JKeys.Generic.Message] = ex.Message
                    };
                    NSULog.Exception(LogTag, $"Action: {action} - Exception: {ex}");
                    SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                }
            }
        }

        private void ActionStartFlash()
        {
            if (_stage == Stage.Upload)
            {
                StartFlashProcess();
            }
        }

        private void ActionDataDone(NetClientData clientData)
        {
            if (_stage == Stage.Upload)
            {
                    _binFile.Flush(true);
                    _binFile.Position = 0;

                    //Compute hash
                    SHA256Managed sha = new SHA256Managed();
                    var hash = BitConverter.ToString(sha.ComputeHash(_binFile)).Replace("-", string.Empty);

                    _binFile.Dispose();
                    _binFile = null;

                    JObject jo = new JObject();
                    jo[JKeys.Generic.Target] = JKeys.BinUploader.TargetName;
                    jo[JKeys.Generic.Action] = JKeys.BinUploader.DataDone;
                    jo[JKeys.Generic.Result] = JKeys.Result.Ok;
                    jo[JKeys.Generic.Value] = hash;
                    SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
            }
        }

        private void ActionData(NetClientData clientData, JObject data)
        {
                if (_stage == Stage.Upload)
                {
                    var bindata = Convert.FromBase64String((string)data[JKeys.Generic.Value]);
                    _binFile.Write(bindata, 0, bindata.Length);
                    JObject jo = new JObject
                    {
                        [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                        [JKeys.Generic.Action] = JKeys.BinUploader.Data,
                        [JKeys.Generic.Status] = (int)data[JKeys.Generic.Status],
                        [JKeys.Generic.Result] = JKeys.Result.Ok
                    };
                    SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                }
                else
                {
                    //Report error
                    NSULog.Debug(LogTag, "Not in progress");
                }
        }

        private void ActionStartUpload(NetClientData clientData)
        {
                if (_stage == Stage.None)
                {
                    CleanUp();
                    _binFile = new FileStream(BinFilePath, FileMode.Create, FileAccess.ReadWrite);
                    _stage = Stage.Upload;
                    _clientData = clientData;
                    JObject jo = new JObject
                    {
                        [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                        [JKeys.Generic.Action] = JKeys.BinUploader.StartUpload,
                        [JKeys.Generic.Result] = JKeys.Result.Ok
                    };
                    SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                }
                else
                {
                    JObject jo = new JObject
                    {
                        [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                        [JKeys.Generic.Action] = JKeys.BinUploader.StartUpload,
                        [JKeys.Generic.Result] = JKeys.Result.Error,
                        [JKeys.Generic.ErrCode] = JKeys.ErrCodes.BinUploader.FlashInProgress
                    };
                    SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);

                    NSULog.Debug(LogTag, "Another user is flashing???");
                    NSULog.Debug(LogTag, $"Current flassher: {_clientData?.ToString()}");
                    NSULog.Debug(LogTag, $"New flasher: {clientData?.ToString()}");
                }
        }

        private void ActionAbort(NetClientData clientData)
        {
            if (_stage == Stage.Upload)
            {
                CleanUp();
                _stage = Stage.None;
                _binFile = null;
                JObject jo = new JObject
                {
                    [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                    [JKeys.Generic.Action] = JKeys.BinUploader.Abort,
                    [JKeys.Generic.Result] = JKeys.Result.Ok
                };
                SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
            }
        }

        private void CleanUp()
        {
            _retryCount = 0;
            _stage = Stage.None;
            _binFile?.Dispose();
            _binFile = null;
            if (File.Exists(BinFilePath))
            {
                File.Delete(BinFilePath);
            }
        }

        private void StartFlashProcess()
        {
            _stage = Stage.Write;
            NSULog.Debug(LogTag, "StartFlashProcess()");
            nsusys.cmdCenter.Stop();

            ProcessStartInfo psi = new ProcessStartInfo(_nsuSys.Config.Bossac)
            {
                Arguments = $"-i -d --port={_nsuSys.Config.BossacPort} -U false -e -w -v -b {Path.Combine(_nsuSys.Config.NSUWritablePath, BinFile)} -R",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            NSULog.Debug(LogTag, $"Starting '{psi.FileName}' with arguments '{psi.Arguments}'");
            ReportInfo($"Starting: {psi.FileName}");
            ReportInfo($"Arguments: {psi.Arguments}");

            _process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };
            _process.OutputDataReceived += HandlePrcOutputDataReceived;
            _process.Exited += HandlePrcExited;
            _process.Start();
            _process.BeginOutputReadLine();
        }

        private void HandlePrcOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string input = e.Data.Trim();

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (input.StartsWith("write")) return;
                if (input.StartsWith("read")) return;
                if (input.StartsWith("go")) return;

                NSULog.Debug(LogTag, "bossac: " + input);
                if (input.StartsWith("Write") && input.Contains("to flash"))
                {
                    _stage = Stage.Write;
                    FlashStarted();
                }
                else if (input.StartsWith("Verify") && input.Contains("of flash"))
                {
                    _stage = Stage.Verify;
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
            NSULog.Debug(LogTag, "HandlePrcExited() exit code: " + _process.ExitCode.ToString());
            int exitCode = _process.ExitCode;
            int c = 0;
            while (!_process.HasExited && c < 15)
            {
                NSULog.Debug(LogTag, "!prc.HasExited: Sleeping...");
                System.Threading.Thread.Sleep(250);
                c++;
            }
            if (!_process.HasExited)
            {
                NSULog.Debug(LogTag, "Still !prc.HasExited: Killing...");
                _process.Kill();
            }

            NSULog.Debug(LogTag, "prc.Close();");
            _process.Close();
            _process = null;
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
                JObject jo = new JObject
                {
                    [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                    [JKeys.Generic.Action] = JKeys.BinUploader.StartFlash,
                    [JKeys.Generic.Result] = JKeys.Result.Error,
                    [JKeys.Generic.ErrCode] = JKeys.ErrCodes.BinUploader.BossacError,
                    [JKeys.Generic.Value] = exitCode
                };
                NSULog.Exception(LogTag, "Bossac exit code - " + exitCode.ToString());
                SendToClient(NetClientRequirements.CreateStandartClientOnly(_clientData), jo);

                //Send info about retry
                ReportInfo("Retrying in 3 seconds...");
                //Init repeat
                if (_retryCount < 3)
                {
                    _retryCount++;
                    _retryTimer.Enabled = true;
                }
            }
        }

        private void FlashStarted()
        {
            _stage = Stage.Write;
            NSULog.Debug(LogTag, "Flash Started....");
            JObject jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                [JKeys.Generic.Action] = JKeys.BinUploader.FlashStarted
            };
            SendToClient(NetClientRequirements.CreateStandartClientOnly(_clientData), jo);
        }

        private void VerifyStarted()
        {
            _stage = Stage.Verify;
            JObject jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                [JKeys.Generic.Action] = JKeys.BinUploader.VerifyStarted
            };
            SendToClient(NetClientRequirements.CreateStandartClientOnly(_clientData), jo);
        }

        private void ReportProgress(string input)
        {
            var result = input.Split(' ')
                .Where(s => s.Contains('%'))
                .Select(s => s.Trim('%')).First();

            if (int.TryParse(result, out int res))
            {
                JObject jo = new JObject
                {
                    [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                    [JKeys.Generic.Action] = JKeys.BinUploader.Progress,
                    [JKeys.Generic.Value] = res
                };
                SendToClient(NetClientRequirements.CreateStandartClientOnly(_clientData), jo);
            }
        }

        private void ReportInfo(string input)
        {
            JObject jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                [JKeys.Generic.Action] = JKeys.BinUploader.InfoText,
                [JKeys.Generic.Value] = input
            };
            SendToClient(NetClientRequirements.CreateStandartClientOnly(_clientData), jo);

        }

        private void ReportFinish()
        {
            _stage = Stage.None;
            JObject jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                [JKeys.Generic.Action] = JKeys.BinUploader.FlashDone
            };
            SendToClient(NetClientRequirements.CreateStandartClientOnly(_clientData), jo);
        }
    }
}
