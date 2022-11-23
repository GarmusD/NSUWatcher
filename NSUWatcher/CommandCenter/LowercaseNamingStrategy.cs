using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NSUWatcher.CommandCenter
{
    public class LowercaseNamingStrategy : NamingStrategy
    {
        public static JsonSerializerSettings LowercaseSettings => _lowercaseSettings;

        private static readonly JsonSerializerSettings _lowercaseSettings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new LowercaseNamingStrategy()
            }
        };

        protected override string ResolvePropertyName(string name)
        {
            return name.ToLowerInvariant();
        }
    }
}
