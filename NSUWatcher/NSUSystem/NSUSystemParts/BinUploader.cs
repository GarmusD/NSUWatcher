using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Timers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.MCUCommands;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class BinUploader : NSUSysPartBase
    {
        private enum Stage
        {
            None,
            Upload,
            Write,
            Verify
        };

        private const string BinFile = "nsu.bin";

        public override string[] SupportedTargets => new string[] { JKeys.BinUploader.TargetName };

        private string BinFilePath => Path.Combine("/tmp", BinFile);
        private FileStream _binFile = null;
        private Process _process = null;
        private Stage _stage = Stage.None;
        private readonly Timer _retryTimer;
        private int _retryCount = 0;

        public BinUploader(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer) : base(sys, loggerFactory, serializer, PartType.BinUploader)
        {
            _nsuSys = sys;
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
                _logger.LogDebug("Flash retry timer elapsed.");
                StartFlashProcess();
            }
        }

        public override void Clear()
        {
            // Nothing to do here
        }

        public override bool ProcessCommandFromMcu(IMessageFromMcu command)
        {
            // Nothing to do here
            return false;
        }

        public override IExternalCommandResult ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            ExtCommandResult result;
            try
            {
                switch (command.Action)
                {
                    case JKeys.BinUploader.Abort:
                        result = ExecActionAbort();
                        break;

                    case JKeys.BinUploader.StartUpload:
                        result = ExecActionStartUpload();
                        break;

                    case JKeys.BinUploader.Data:
                        result = ExecActionData(command.Content);
                        break;

                    case JKeys.BinUploader.DataDone:
                        result = ExecActionDataDone();
                        break;
                    case JKeys.BinUploader.StartFlash:
                        result = ActionStartFlash();
                        break;
                    default:
                        string msg = $"Action '{command.Action}' is not implemented.";
                        _logger.LogWarning(msg);
                        result = ExtCommandResult.Failure(JKeys.BinUploader.TargetName, command.Action, msg);
                        break;
                }
            }
            catch (Exception ex)
            {
                result = CleanupAndFail(command.Action, ex.Message);
            }
            return result;
        }

        private ExtCommandResult ActionStartFlash()
        {
            if (_stage == Stage.Upload)
            {
                return StartFlashProcess();
            }
            return ExtCommandResult.Failure(JKeys.BinUploader.TargetName, JKeys.BinUploader.StartFlash, $"Wrogn operation stage. Required: 'Upload', Current: '{_stage}'");
        }

        private ExtCommandResult ExecActionData(string data)
        {
            if (_stage == Stage.Upload)
            {
                try
                {
                    BinUploadDataContent uplData = JsonConvert.DeserializeObject<BinUploadDataContent>(data);
                    var bindata = Convert.FromBase64String(uplData.Content);
                    _binFile?.Write(bindata, 0, bindata.Length);
                    return ExtCommandResult.Success(JKeys.BinUploader.TargetName, JKeys.BinUploader.Data, uplData.Progress.ToString());
                }
                catch (Exception ex)
                {
                    return CleanupAndFail(JKeys.BinUploader.Data, ex.Message);
                }
            }
            else
            {
                //Report error
                _logger.LogError($"Wrogn operation stage. Required: 'Upload', Current: '{_stage}'");
                return ExtCommandResult.Failure(JKeys.BinUploader.TargetName, JKeys.BinUploader.Data, $"Wrogn operation stage. Required: 'Upload', Current: '{_stage}'");
            }
        }

        private ExtCommandResult CleanupAndFail(string action, string errorMessage)
        {
            _logger.LogError($"BinUploader failed with message: '{errorMessage}'.");
            CleanUp();
            return ExtCommandResult.Failure(JKeys.BinUploader.TargetName, action, errorMessage);
        }

        private ExtCommandResult ExecActionDataDone()
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

                return ExtCommandResult.Success(JKeys.BinUploader.TargetName, JKeys.BinUploader.DataDone, hash);
            }
            return ExtCommandResult.Failure(JKeys.BinUploader.TargetName, JKeys.BinUploader.DataDone, $"Wrogn operation stage. Required: 'Upload', Current: '{_stage}'");
        }

        private ExtCommandResult ExecActionStartUpload()
        {
            if (_stage == Stage.None)
            {
                try
                {
                    CleanUp();
                    _binFile = new FileStream(BinFilePath, FileMode.Create, FileAccess.ReadWrite);
                    _stage = Stage.Upload;
                    return ExtCommandResult.Success(JKeys.BinUploader.TargetName, JKeys.BinUploader.StartUpload);
                }
                catch (Exception ex)
                {
                    return CleanupAndFail(JKeys.BinUploader.StartUpload, ex.Message);
                }
            }
            _logger.LogDebug("Another user is flashing???");
            return ExtCommandResult.Failure(JKeys.BinUploader.TargetName, JKeys.BinUploader.DataDone, $"Wrogn operation stage. Required: 'None', Current: '{_stage}'");
        }

        private ExtCommandResult ExecActionAbort()
        {
            if (_stage == Stage.Upload)
            {
                CleanUp();
                return ExtCommandResult.Success(JKeys.BinUploader.TargetName, JKeys.BinUploader.Abort);
            }
            return ExtCommandResult.Failure(JKeys.BinUploader.TargetName, JKeys.BinUploader.Abort, "Not uploading");
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

        private ExtCommandResult StartFlashProcess()
        {
            try
            {
                _stage = Stage.Write;
                _logger.LogDebug("StartFlashProcess()");

                ProcessStartInfo psi = new ProcessStartInfo(_nsuSys.Config.Bossac?.Cmd)
                {
                    Arguments = $"-i -d --port={_nsuSys.Config.Bossac?.Port} -U false -e -w -v -b {Path.Combine("/tmp", BinFile)} -R",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _logger.LogDebug($"Starting '{psi.FileName}' with arguments '{psi.Arguments}'");
                ReportInfo($"Starting: {psi.FileName}");
                ReportInfo($"Arguments: {psi.Arguments}");

                _process = new Process
                {
                    StartInfo = psi,
                    EnableRaisingEvents = true
                };
                _process.OutputDataReceived += HandlePrcOutputDataReceived;
                _process.Exited += HandlePrcExited;
                var result = _process.Start();
                if (result)
                {
                    _process.BeginOutputReadLine();
                    return ExtCommandResult.Success(JKeys.BinUploader.TargetName, JKeys.BinUploader.StartFlash);
                }
                else
                {
                    _process?.Dispose();
                    _stage = Stage.None;
                    return ExtCommandResult.Failure(JKeys.BinUploader.TargetName, JKeys.BinUploader.StartFlash, "Failed to start Bossac process.");
                }
            }
            catch (Exception ex)
            {
                _process?.Dispose();
                _stage = Stage.None;
                return ExtCommandResult.Failure(JKeys.BinUploader.TargetName, JKeys.BinUploader.StartFlash, ex.Message);
            }
        }

        private void HandlePrcOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string input = e.Data.Trim();

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (input.StartsWith("write")) return;
                if (input.StartsWith("read")) return;
                if (input.StartsWith("go")) return;

                _logger.LogDebug("bossac: " + input);
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
            _logger.LogDebug("HandlePrcExited() exit code: " + _process.ExitCode.ToString());
            int exitCode = _process.ExitCode;
            int c = 0;
            while (!_process.HasExited && c++ < 15)
            {
                _logger.LogDebug("!prc.HasExited: Sleeping...");
                System.Threading.Thread.Sleep(250);
            }

            if (!_process.HasExited)
            {
                _logger.LogDebug("Still !prc.HasExited: Killing...");
                _process.Kill();
            }

            _logger.LogDebug("prc.Close();");
            _process.Close();
            _process.Dispose();
            _process = null;
            if (exitCode == 0)
            {
                _logger.LogDebug("Starting CmdCenter...");
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
                _logger.LogError($"Bossac exit code - {exitCode}");
                ///SendToClient(NetClientRequirements.CreateStandartClientOnly(_clientData), jo);

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
            _logger.LogDebug("Flash Started....");
            //TODO Report FlashStarted
            JObject jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                [JKeys.Generic.Action] = JKeys.BinUploader.FlashStarted
            };
            ///SendToClient(NetClientRequirements.CreateStandartClientOnly(_clientData), jo);
        }

        private void VerifyStarted()
        {
            _stage = Stage.Verify;
            //TODO Report VerifyStarted
            JObject jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                [JKeys.Generic.Action] = JKeys.BinUploader.VerifyStarted
            };
            ///SendToClient(NetClientRequirements.CreateStandartClientOnly(_clientData), jo);
        }

        private void ReportProgress(string input)
        {
            var result = input.Split(' ')
                .Where(s => s.Contains('%'))
                .Select(s => s.Trim('%')).First();

            if (int.TryParse(result, out int res))
            {
                //TODO Report Progress
                JObject jo = new JObject
                {
                    [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                    [JKeys.Generic.Action] = JKeys.BinUploader.Progress,
                    [JKeys.Generic.Value] = res
                };
                ///SendToClient(NetClientRequirements.CreateStandartClientOnly(_clientData), jo);
            }
        }

        private void ReportInfo(string input)
        {
            //TODO Report Info
            JObject jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                [JKeys.Generic.Action] = JKeys.BinUploader.InfoText,
                [JKeys.Generic.Value] = input
            };
            ///SendToClient(NetClientRequirements.CreateStandartClientOnly(_clientData), jo);

        }

        private void ReportFinish()
        {
            _stage = Stage.None;
            //TODO Report Finish
            JObject jo = new JObject
            {
                [JKeys.Generic.Target] = JKeys.BinUploader.TargetName,
                [JKeys.Generic.Action] = JKeys.BinUploader.FlashDone
            };
            ///SendToClient(NetClientRequirements.CreateStandartClientOnly(_clientData), jo);
        }

        public override IEnumerator GetEnumerator<T>()
        {
            return null;
        }
    }
}
