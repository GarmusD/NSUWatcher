using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.Serializer;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NSUWatcher.Transport.TestTransport.Commands
{
    internal class TSensorCommands : TestCommandsBase
    {
        private List<TSensorTemplate> _sensors;
        private Random _random = new Random();
        private NsuSerializer _serializer = new NsuSerializer();

        public TSensorCommands(CommandSender commandSender, ILoggerFactory loggerFactory) : base(commandSender, loggerFactory.CreateLoggerShort<TSensorCommands>())
        {
            _sensors = new List<TSensorTemplate>();
            foreach (var item in TSensorAdresses.Adresses)
            {
                _sensors.Add(new TSensorTemplate(item, GetRandomTemp(20)));
            }
        }

        public override bool ExecCommand(JObject command)
        {
            if ((string)command[JKeys.Generic.Target] != JKeys.TempSensor.TargetName ) return false;

            return true;
        }

        /// <summary>
        /// Imitate Temperature Sensor changes
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                // delay before run to enable process the Snapshot commands
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                while (!cancellationToken.IsCancellationRequested)
                {
                    TSensorTemplate sensor = RandomizeTSensor();
                    _sender.Send(_serializer.Serialize(sensor));
                    await Task.Delay(_random.Next((int)TimeSpan.FromSeconds(10).TotalMilliseconds), cancellationToken);
                }
            }
            catch (OperationCanceledException) { }
            return;
        }

        private TSensorTemplate RandomizeTSensor()
        {
            TSensorTemplate sensor = _sensors[_random.Next(_sensors.Count)];
            sensor.Temperature += GetRandomTemp();
            sensor.ReadErrors += GetRandomReadErrorAddition();
            return sensor;
        }

        private double GetRandomTemp(double multiplier = 3.0)
        {
            bool sign = (25 - _random.Next(51)) <= 0;
            double result = Math.Round(_random.NextDouble() * multiplier, 1);
            return sign ? 0 - result : result;
        }

        private int GetRandomReadErrorAddition()
        {
            if(_random.Next(5000) > 4000) return 1;
            return 0;
        }
    }
}
