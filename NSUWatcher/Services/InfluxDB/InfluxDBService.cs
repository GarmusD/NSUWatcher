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
using System.Diagnostics;
using System.IO;
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
            _logger.LogDebug($"Using token: {_config.Token}");
            _logger.LogDebug($"Using url: {_config.Url}");
            _logger.LogDebug($"Using bucket: {_config.Bucket}");
            _logger.LogDebug($"Using org: {_config.Org}");
            _influxDbClient = new InfluxDBClient(_config!.Url, _config!.Token);
            
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
            var tsData = _nsuSys.GetEntityData<ITempSensorDataContract>();
            if(tsData != null)
                foreach (var data in tsData)
                {
                    _sensorEntities.Add(new TemperatureSensorEntity(data.Temperature) 
                    { 
                        SensorName = data.Name,
                        SensorType = DataEntityType.DS18B20,
                        SensorID = TempSensor.AddrToString(data.SensorID),
                        Timing = _config!.Timing.TSensor
                    });
                }
            var ktypeData = _nsuSys.GetEntityData<IKTypeDataContract>();
            if(ktypeData != null)
                foreach (var data in ktypeData)
                {
                    _sensorEntities.Add(new TemperatureSensorEntity(data.Temperature)
                    {
                        SensorName = data.Name,
                        SensorType = DataEntityType.KType,
                        Timing = _config!.Timing.TSensor
                    });
                }
            _logger.LogDebug($"ReloadEntityData(): SensorEntities: {_sensorEntities.Count}");
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

        private Task WriteDataAsync(int minute)
        {
            return Task.Run(() => 
            {
                int writesCount = 0;
                using var writeApi = _influxDbClient?.GetWriteApi();
                foreach (var sensorEntity in _sensorEntities)
                {
                    if (minute % sensorEntity.Timing == 0)
                    {
                        var pointData = GetTempSensorPointData(sensorEntity);
                        try
                        {
                            writesCount++;
                            writeApi?.WritePoint(pointData, bucket: _config!.Bucket, org: _config!.Org);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"WriteDataAsync(): {ex.Message}");
                        }
                    }
                }

                _logger.LogDebug($"WriteDataAsync() done with {writesCount} writes.");
            });
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
