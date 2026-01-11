using System.ComponentModel.DataAnnotations;

namespace OmniStream.Analytics.Worker.Configuration
{
    public class RedisSettings
    {
        [Required]
        public string ConnectionString { get; set; } = default!;
        /// <summary>
        /// Analytics window size in seconds (e.g., 600 = 10 minutes)
        /// Data older than this time will be deleted and will not be included in calculations
        /// </summary>
        public int WindowSeconds { get; set; } = default!;

        /// <summary>
        /// Key lifetime in minutes (TTL)
        /// Must be greater than WindowSeconds to prevent data from disappearing prematurely
        /// </summary>
        public int TtlMinutes { get; set; } = default!;
    }
}