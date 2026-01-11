using System.ComponentModel.DataAnnotations;

namespace OmniStream.Analytics.Worker.Configuration
{
    internal class RedisSettings
    {
        [Required]
        public string ConnectionString { get; set; } = default!;
    }
}