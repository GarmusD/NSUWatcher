using Newtonsoft.Json;
using NSU.Shared.DataContracts;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.MCUCommands;
using Serilog;
using System;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{

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

		protected NSUSysPartBase(NsuSystem sys, ILogger logger, INsuSerializer serializer, PartType type)
        {
            _nsuSys = sys ?? throw new ArgumentNullException(nameof(sys), "Instance of NSUSys cannot be null.");
            _logger = logger.ForContext(this.GetType()) ?? throw new ArgumentNullException(nameof(logger), "Instance of OLogger cannot be null.");
            _serializer = serializer;
            _partType = type;
        }
		
        protected void LogNotImplementedCommand(IMessageFromMcu command)
        {
            _logger.Warning($"Not implemented message from mcu: {command.GetType().FullName}");
        }

        protected void LogNotImplementedCommand(IExternalCommand command)
        {
            _logger.Warning($"Not implemented external command: {command.Target}:{command.Action}");
        }

        protected void OnPropertyChanged(INSUSysPartDataContract source, string property)
        {
            _nsuSys.OnStatusChanged(source, property);
        }

        public abstract void Clear();
        public abstract void ProcessCommandFromMcu(IMessageFromMcu command);
		public abstract IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context);

    }
}

