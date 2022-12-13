namespace NSUWatcher.Services.InfluxDB
{
    internal class Config
    {
        public string Url { get; set; }
        public string Token { get; set; }
        public string Bucket { get; set; }
        public string Org { get; set; }
        public Timing Timing { get; set; } = new Timing();
    }

    internal class Timing
    {
        public int TSensor { get; set; } = 15;
    }
}
