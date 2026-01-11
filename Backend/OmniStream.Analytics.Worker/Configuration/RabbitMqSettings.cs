using System.ComponentModel.DataAnnotations;

namespace OmniStream.Analytics.Worker.Configuration
{
    public class RabbitMqSettings
    {
        [Required]
        public string Host { get; set; } = default!;
        [Required]
        public string Username { get; set; } = default!;
        [Required]
        public string Password { get; set; } = default!;
        [Required]
        public string QueueName { get; set; } = default!;
    }
}