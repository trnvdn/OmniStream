namespace OmniStream.Analytics.Worker.Configuration
{
    internal class OmniStreamSettings
    {
        public RabbitMqSettings RabbitMQ { get; set; } = new();
        public RedisSettings Redis { get; set; } = new();
    }
}
