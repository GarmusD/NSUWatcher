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
using System.Threading;
using System.Threading.Tasks;

namespace NSUWatcher.Services.InfluxDB
{
#nullable enable
    public class InfluxDBService : BackgroundService
    {
        private readonly InfluxDBClient? _influxDbClient = null;
        private readonly ILogger _logger;
        private readonly INsuSystem _nsuSys;

        private readonly string? _bucket;
        private readonly string? _org;
        private readonly System.Timers.Timer? _oneMinuteTimer = null;
        private readonly List<TemperatureSensorEntity> _sensorEntities = new List<TemperatureSensorEntity>();

        public InfluxDBService(INsuSystem nsuSystem, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLoggerShort<InfluxDBService>() ?? NullLoggerFactory.Instance.CreateLoggerShort<InfluxDBService>();
            _nsuSys = nsuSystem;

            string? token = configuration["InfluxDB:token"];
            string? url = configuration["InfluxDB:url"];
            _bucket = configuration["InfluxDB:bucket"];
            _org = configuration["InfluxDB:org"];

            if (!ValidateConfig(token))
            {
                _logger.LogWarning("InfluxDB is not configured and will not run. Required values in 'InfluxDB' section is: 'url', 'token', 'bucket' and 'org'.");
                return;
            }

            _influxDbClient = new InfluxDBClient(url, token);
            
            _oneMinuteTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
            _oneMinuteTimer.Elapsed += (s, e) => 
            { 
                _ = WriteDataAsync(DateTime.Now.Minute);
            };
        }

        private bool ValidateConfig(string? token)
        {
            return !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(_bucket) && !string.IsNullOrEmpty(_org);
        }

        private PointData GetTempSensorPointData(TemperatureSensorEntity tempSensorData)
        {
            return PointData
                .Measurement("temperature")
                .Tag("name", tempSensorData.SensorName)
                .Tag("id", tempSensorData.SensorID)
                .Tag("type", tempSensorData.SensorType.ToString())
                .Field("temp", tempSensorData.Temperature)
                .Field("hum", tempSensorData.Humidity)
                .Field("pre", tempSensorData.Pressure)
                .Timestamp(DateTime.Now, WritePrecision.S)
                ;
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
                        SensorID = TempSensor.AddrToString(data.SensorID)
                    });
                }
            var ktypeData = _nsuSys.GetEntityData<IKTypeDataContract>();
            if(ktypeData != null)
                foreach (var data in ktypeData)
                {
                    _sensorEntities.Add(new TemperatureSensorEntity(data.Temperature)
                    {
                        SensorName = data.Name,
                        SensorType = DataEntityType.KType
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
                using var writeApi = _influxDbClient?.GetWriteApi();
                foreach (var sensorEntity in _sensorEntities)
                {
                    if(sensorEntity.Timing % minute == 0)
                    {
                        var pointData = GetTempSensorPointData(sensorEntity);
                        try
                        {
                            writeApi?.WritePoint(pointData, bucket: _bucket, org: _org);
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError($"WriteDataAsync(): {ex.Message}");
                        }
                    }    
                }   
            });
        }

        private async Task CalibrateMinute(CancellationToken cancellationToken)
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime awaitTo = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute + 1, 0, 100);
                await Task.Delay((int)(awaitTo - now).TotalMilliseconds, cancellationToken);
                _oneMinuteTimer!.Enabled = true;
            }
            catch (OperationCanceledException) { }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_influxDbClient == null) return;
            _logger.LogDebug("ExecuteAsync()");
             
            _ = CalibrateMinute(stoppingToken);
            _nsuSys.SystemStatusChanged += ((s, e) => ReloadEntityData());
            if (_nsuSys.CurrentStatus == NsuSystemStatus.Ready)
                ReloadEntityData();
            _nsuSys.EntityStatusChanged += (s, e) => UpdateEntityData((INSUSysPartDataContract)s, e.PropertyName);

            // Wait for stoppingToken signal
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
            var reg = stoppingToken.Register(() => completionSource.SetResult(true));
            await Task.WhenAny(completionSource.Task);

            _logger.LogTrace("ExecuteAsync() Done.");
        }
    }
#nullable disable
}
