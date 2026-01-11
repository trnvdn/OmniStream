using Microsoft.Extensions.Options;
using OmniStream.Analytics.Worker.Configuration;
using StackExchange.Redis;

namespace OmniStream.Analytics.Worker.Services
{
    public class RedisMetricsRepository
    {
        private readonly IDatabase _db;
        private readonly RedisSettings _settings;

        public RedisMetricsRepository(IConnectionMultiplexer redis, IOptions<OmniStreamSettings> settings)
        {
            _db = redis.GetDatabase();
            _settings = settings.Value.Redis;
        }

        public async Task AddMetricAsync(Guid deviceId, string metricType, double value)
        {
            var key = $"metrics:{deviceId}:{metricType}";
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await _db.SortedSetAddAsync(key, value, now);

            await _db.KeyExpireAsync(key, TimeSpan.FromMinutes(_settings.TtlMinutes));

            var windowStart = now - _settings.WindowSeconds;
            await _db.SortedSetRemoveRangeByScoreAsync(key, double.NegativeInfinity, windowStart);
        }

        public async Task<double> GetAverageAsync(Guid deviceId, string metricType)
        {
            var key = $"metrics:{deviceId}:{metricType}";
            var values = await _db.SortedSetRangeByRankAsync(key);

            if (values.Length == 0) return 0;
            return values.Select(v => (double)v).Average();
        }
    }
}
