namespace OmniStream.Analytics.Worker.Configuration
{
    public class OmniStreamSettings
    {
        public RabbitMqSettings RabbitMQ { get; set; } = new();
        public RedisSettings Redis { get; set; } = new();
    }
}
