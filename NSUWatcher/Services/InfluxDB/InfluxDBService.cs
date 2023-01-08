using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSU.Shared.DataContracts;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSUWatcher.Services.InfluxDB
{
#nullable enable
    /// <summary>
    /// A BackgroundService to write temperature and status data to InfluxDB
    /// </summary>
    public class InfluxDBService : BackgroundService
    {
        private readonly InfluxDBClient? _influxDbClient = null;
        private readonly ILogger _logger;
        private readonly INsuSystem _nsuSys;
        private readonly Config? _config;
        private readonly System.Timers.Timer? _oneMinuteTimer = null;
        private readonly List<TemperatureSensorEntity> _sensorEntities = new List<TemperatureSensorEntity>();

        public InfluxDBService(INsuSystem nsuSystem, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLoggerShort<InfluxDBService>() ?? NullLoggerFactory.Instance.CreateLoggerShort<InfluxDBService>();
            _nsuSys = nsuSystem;

            _config = configuration.GetSection("InfluxDB").Get<Config>();
            if (!ValidateConfig())
            {
                _logger.LogWarning("InfluxDB is not configured and will not run. Required values in 'InfluxDB' section is: 'url', 'token', 'bucket' and 'org'.");
                return;
            }
            _logger.LogDebug($"InfluxDbConfig.Url: {_config!.Url}");
            _logger.LogDebug($"InfluxDbConfig.Token: {_config!.Token}");
            _logger.LogDebug($"InfluxDbConfig.Bucket: {_config!.Bucket}");
            _logger.LogDebug($"InfluxDbConfig.Org: {_config!.Org}");

            InfluxDBClientOptions options = new InfluxDBClientOptions(_config!.Url)
            { 
                Token = _config.Token,
                Bucket = _config.Bucket,
                Org = _config.Org,
            };
            _influxDbClient = new InfluxDBClient(options);
            
            _oneMinuteTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _oneMinuteTimer.Elapsed += (s, e) => 
            { 
                _ = WriteDataAsync(DateTime.Now.Minute);
            };
        }

        private bool ValidateConfig()
        {
            return 
                _config != null && 
                !string.IsNullOrEmpty(_config.Token) && 
                !string.IsNullOrEmpty(_config.Bucket) && 
                !string.IsNullOrEmpty(_config.Org);
        }

        private void ReloadEntityData()
        {
            _sensorEntities.Clear();
            ReloadTempSensorData();
            ReloadKTypeData();
            _logger.LogDebug($"ReloadEntityData(): SensorEntities: {_sensorEntities.Count}");
        }
        
        private void ReloadTempSensorData()
        {
            var tsData = _nsuSys.GetEntityData<ITempSensorDataContract>();
            if (tsData != null)
                foreach (var data in tsData)
                {
                    if (data.Enabled)
                    {
                        _sensorEntities.Add(new TemperatureSensorEntity(data.Temperature)
                        {
                            SensorName = data.Name,
                            SensorType = DataEntityType.DS18B20,
                            SensorID = TempSensor.AddrToString(data.SensorID),
                            Timing = _config!.Timing.TSensor
                        });
                    }
                }
        }

        private void ReloadKTypeData()
        {
            var ktypeData = _nsuSys.GetEntityData<IKTypeDataContract>();
            if (ktypeData != null)
                foreach (var data in ktypeData)
                {
                    _sensorEntities.Add(new TemperatureSensorEntity(data.Temperature)
                    {
                        SensorName = data.Name,
                        SensorType = DataEntityType.KType,
                        SensorID = data.Name,
                        Timing = _config!.Timing.TSensor
                    });
                }
        }

        private void UpdateEntityData(INSUSysPartDataContract s, string propertyName)
        {
            switch (s)
            { 
                case ITempSensorDataContract tempSensorData:
                    if(propertyName == nameof(tempSensorData.Temperature))
                        UpdateDS18B20Data(tempSensorData);
                    return;
                case IKTypeDataContract ktypeData:
                    if(propertyName == nameof(ktypeData.Temperature))
                        UpdateKTypeData(ktypeData);
                    return;
            };
        }

        private void UpdateDS18B20Data(ITempSensorDataContract tempSensorData)
        {
            var entity = _sensorEntities.Find(x => x.SensorType == DataEntityType.DS18B20 && x.SensorName == tempSensorData.Name);
            if (entity != null)
                entity.Temperature = tempSensorData.Temperature;
        }

        private void UpdateKTypeData(IKTypeDataContract ktypeData)
        {
            var entity = _sensorEntities.Find(x => x.SensorType == DataEntityType.KType && x.SensorName == ktypeData.Name);
            if(entity != null)
                entity.Temperature = ktypeData.Temperature;
        }

        /// <summary>
        /// Called at the start of minute. Writes to DB based on Timig value of dataEntity
        /// </summary>
        /// <param name="minute"></param>
        /// <returns></returns>
        private Task WriteDataAsync(int minute)
        {
            return Task.Run(() =>
            {
                var dataPoints = _sensorEntities.FindAll(x => minute % x.Timing == 0).Select(x => GetTempSensorPointData(x)).ToList();
                if (dataPoints?.Any() == true)
                {
                    using var writeApi = _influxDbClient?.GetWriteApi();
                    if (writeApi != null)
                    {
                        CatchWriteApiErrorEvents(writeApi);

                        writeApi.WritePoints(dataPoints);
                        _logger.LogDebug($"WriteDataAsync() done for {dataPoints.Count} data points.");
                    }
                    else
                    {
                        _logger.LogError("_influxDbClient?.GetWriteApi() returned null.");
                    }
                }
            });
        }

        private void CatchWriteApiErrorEvents(WriteApi writeApi)
        {
            writeApi!.EventHandler += (s, e) => 
            {
                switch (e)
                {
                    // unhandled exception from server
                    case WriteErrorEvent error:
                        _logger.LogError($"WriteErrorEvent: {error.Exception}, LineProtocol: {error.LineProtocol}");
                        break;

                    // retrievable error from server
                    case WriteRetriableErrorEvent error:
                        _logger.LogError($"WriteRetriableErrorEvent: {error.Exception}, LineProtocol: {error.LineProtocol}");
                        break;

                    // runtime exception in background batch processing
                    case WriteRuntimeExceptionEvent error:
                        _logger.LogError($"WriteRuntimeExceptionEvent: {error.Exception}");
                        break;
                }
            };
        }

        private PointData GetTempSensorPointData(TemperatureSensorEntity tempSensorData)
        {
            var data = PointData
                .Measurement("tphsensor")
                .Tag("name", tempSensorData.SensorName)
                .Tag("id", tempSensorData.SensorID)
                .Tag("type", tempSensorData.SensorType.ToString())
                .Field("temperature", tempSensorData.Temperature)
                .Field("humidity", tempSensorData.Humidity)
                .Field("pressure", tempSensorData.Pressure)
                .Timestamp(DateTime.UtcNow, WritePrecision.S)
                ;
            return data;
        }

        /// <summary>
        /// Call data writer at the begining of each minute
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task CalibrateToMinuteStart(CancellationToken cancellationToken)
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime awaitTo = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute + 1, 0, 100);
                await Task.Delay((int)(awaitTo - now).TotalMilliseconds, cancellationToken);
                _oneMinuteTimer!.Start();
                _ = WriteDataAsync(DateTime.Now.Minute);
            }
            catch (OperationCanceledException) { }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // If client is null, there is no point to run
            if (_influxDbClient == null) return;

            _logger.LogDebug("ExecuteAsync()");
            
            _ = CalibrateToMinuteStart(stoppingToken);
            _nsuSys.SystemStatusChanged += ((s, e) => ReloadEntityData());
            if (_nsuSys.CurrentStatus == NsuSystemStatus.Ready)
                ReloadEntityData();
            _nsuSys.EntityStatusChanged += (s, e) => UpdateEntityData((INSUSysPartDataContract)s, e.PropertyName);

            // Wait for stoppingToken signal
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
            var reg = stoppingToken.Register(() => completionSource.SetResult(true));
            await Task.WhenAny(completionSource.Task);

            _oneMinuteTimer?.Stop();
            _logger.LogTrace("ExecuteAsync() Done.");
        }
    }
#nullable disable
}
