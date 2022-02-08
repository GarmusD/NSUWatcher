namespace NSUWatcher.NSUSystem.NSUSystemParts
{
	public static class PartsConsts
	{
		public const int MAX_NAME_LENGTH = 31;//32 position for Zero in c++
		public const int MAX_TEMP_SENSORS = 64;
		public const int MAX_SWITCHES = 16;
		public const int MAX_TEMP_TRIGGERS = 16;
		public const int MAX_CIRC_PUMPS = 8;
		public const int MAX_COLLECTORS = 8;		
		public const int MAX_COMFORT_ZONES = 32;
		public const int MAX_KTYPES = 1;
		public const int MAX_KATILAS = 1;
	}

	public enum PartsTypes
	{
		Unknown,
		System,
		TSensors,
		Switches,
		RelayModules,
		TempTriggers,
		CircPumps,
		Collectors,
		ComfortZones,
		KTypes,
		WaterBoilers,
		WoodBoilers,
		Vacation,
		Scenarios,
		FileUploader,
        UserCommand
	}
}

