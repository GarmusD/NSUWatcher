using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using NSU.Shared.DataContracts;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.NsuUsers;
using System;
using System.Collections;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
#nullable enable
    public abstract class NSUSysPartBase
    {
        [JsonIgnore]
        public abstract string[] SupportedTargets { get; }
        [JsonIgnore]
        public PartType PartType => _partType;
        
        protected ILogger _logger;
        protected NsuSystem _nsuSys;
        protected readonly INsuSerializer _serializer;

        private readonly PartType _partType;

		protected NSUSysPartBase(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer, PartType type)
        {
            _nsuSys = sys ?? throw new ArgumentNullException(nameof(sys), "Instance of NSUSys cannot be null.");
            _logger = loggerFactory.CreateLogger(this.GetType().Name) ?? NullLoggerFactory.Instance.CreateLogger(this.GetType().Name);
            _serializer = serializer;
            _partType = type;
        }
		
        protected void LogNotImplementedCommand(IMessageFromMcu command)
        {
            _logger.LogWarning($"Not implemented message from mcu: {command.GetType().FullName}");
        }

        protected void LogNotImplementedCommand(IExternalCommand command)
        {
            _logger.LogWarning($"Not implemented external command: {command.Target}:{command.Action}");
        }

        protected void OnPropertyChanged(INSUSysPartDataContract source, string property)
        {
            _nsuSys.OnStatusChanged(source, property);
        }

        public abstract void Clear();
        public abstract bool ProcessCommandFromMcu(IMessageFromMcu command);
		public abstract IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context);

        public abstract IEnumerable? GetEnumerator<T>();
    }
#nullable disable
}

