using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSU.Shared.DataContracts;
using NSUWatcher.Interfaces;
using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSUWatcher.Services.InfluxDB
{
#nullable enable
    public class InfluxDBService : IHostedService
    {
        private readonly InfluxDBClient? _influxDbClient = null;
        private readonly ILogger _logger;

        private readonly string? _bucket;
        private readonly string? _org;

        public InfluxDBService(INsuSystem nsuSystem, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLoggerShort<InfluxDBService>() ?? NullLoggerFactory.Instance.CreateLoggerShort<InfluxDBService>();
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
            nsuSystem.StatusChanged += NsuSystem_StatusChanged;
        }

        private bool ValidateConfig(string? token)
        {
            return !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(_bucket) && !string.IsNullOrEmpty(_org);
        }

        private void NsuSystem_StatusChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            PointData? pointData = sender switch
            {
                ITempSensorDataContract tempSensorData => GetTempSensorPointData(tempSensorData),
                _ => null
            };

            using var writeApi = _influxDbClient?.GetWriteApi();
            if (pointData != null)
                writeApi?.WritePoint(pointData, bucket: _bucket, org: _org);


        }

        private PointData GetTempSensorPointData(ITempSensorDataContract tempSensorData)
        {
            return PointData
                .Measurement("temperature")
                .Tag("name", tempSensorData.Name)
                .Field("temp", tempSensorData.Temperature)
                .Field("hum", 0)
                .Field("pre", 0)
                .Timestamp(DateTime.Now, WritePrecision.S)
                ;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
#nullable disable
}
