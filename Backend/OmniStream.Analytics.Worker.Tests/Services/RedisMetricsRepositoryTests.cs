using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using OmniStream.Analytics.Worker.Configuration;
using OmniStream.Analytics.Worker.Services;
using StackExchange.Redis;

namespace OmniStream.Analytics.Worker.Tests.Services;

[TestFixture]
public class RedisMetricsRepositoryTests
{
    private Mock<IConnectionMultiplexer> _redisMock = null!;
    private Mock<IDatabase> _databaseMock = null!;
    private IOptions<OmniStreamSettings> _options = null!;
    private RedisMetricsRepository _repository = null!;

    private const int WindowSeconds = 300;
    private const int TtlMinutes = 10;

    [SetUp]
    public void SetUp()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();

        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                  .Returns(_databaseMock.Object);

        var settings = new OmniStreamSettings
        {
            Redis = new RedisSettings
            {
                ConnectionString = "localhost:6379",
                WindowSeconds = WindowSeconds,
                TtlMinutes = TtlMinutes
            }
        };

        _options = Options.Create(settings);
        _repository = new RedisMetricsRepository(_redisMock.Object, _options);
    }

    [Test]
    public async Task AddMetricAsync_ShouldAddValueToSortedSet()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var metricType = "cpu_usage";
        var value = 75.5;
        var expectedKey = $"metrics:{deviceId}:{metricType}";

        _databaseMock.Setup(d => d.SortedSetAddAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<double>(),
            It.IsAny<SortedSetWhen>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _databaseMock.Setup(d => d.KeyExpireAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<ExpireWhen>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _databaseMock.Setup(d => d.SortedSetRemoveRangeByScoreAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<Exclude>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(0);

        // Act
        await _repository.AddMetricAsync(deviceId, metricType, value);

        // Assert
        _databaseMock.Verify(d => d.SortedSetAddAsync(
            expectedKey,
            value,
            It.IsAny<double>(),
            It.IsAny<SortedSetWhen>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Test]
    public async Task AddMetricAsync_ShouldSetKeyExpiration()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var metricType = "memory_usage";
        var value = 60.0;
        var expectedKey = $"metrics:{deviceId}:{metricType}";

        _databaseMock.Setup(d => d.SortedSetAddAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<double>(),
            It.IsAny<SortedSetWhen>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _databaseMock.Setup(d => d.KeyExpireAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<ExpireWhen>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _databaseMock.Setup(d => d.SortedSetRemoveRangeByScoreAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<Exclude>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(0);

        // Act
        await _repository.AddMetricAsync(deviceId, metricType, value);

        // Assert
        _databaseMock.Verify(d => d.KeyExpireAsync(
            expectedKey,
            TimeSpan.FromMinutes(TtlMinutes),
            It.IsAny<ExpireWhen>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Test]
    public async Task AddMetricAsync_ShouldRemoveOldValuesOutsideWindow()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var metricType = "disk_io";
        var value = 45.0;
        var expectedKey = $"metrics:{deviceId}:{metricType}";

        _databaseMock.Setup(d => d.SortedSetAddAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<double>(),
            It.IsAny<SortedSetWhen>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _databaseMock.Setup(d => d.KeyExpireAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<ExpireWhen>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _databaseMock.Setup(d => d.SortedSetRemoveRangeByScoreAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<Exclude>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(3);

        // Act
        await _repository.AddMetricAsync(deviceId, metricType, value);

        // Assert
        _databaseMock.Verify(d => d.SortedSetRemoveRangeByScoreAsync(
            expectedKey,
            double.NegativeInfinity,
            It.IsAny<double>(),
            It.IsAny<Exclude>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Test]
    public async Task GetAverageAsync_WithValues_ShouldReturnCorrectAverage()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var metricType = "cpu_usage";
        var expectedKey = $"metrics:{deviceId}:{metricType}";
        var values = new RedisValue[] { 10.0, 20.0, 30.0, 40.0 };

        _databaseMock.Setup(d => d.SortedSetRangeByRankAsync(
            expectedKey,
            It.IsAny<long>(),
            It.IsAny<long>(),
            It.IsAny<Order>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(values);

        // Act
        var result = await _repository.GetAverageAsync(deviceId, metricType);

        // Assert
        result.Should().Be(25.0);
    }

    [Test]
    public async Task GetAverageAsync_WithNoValues_ShouldReturnZero()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var metricType = "cpu_usage";
        var expectedKey = $"metrics:{deviceId}:{metricType}";

        _databaseMock.Setup(d => d.SortedSetRangeByRankAsync(
            expectedKey,
            It.IsAny<long>(),
            It.IsAny<long>(),
            It.IsAny<Order>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(Array.Empty<RedisValue>());

        // Act
        var result = await _repository.GetAverageAsync(deviceId, metricType);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task GetAverageAsync_WithSingleValue_ShouldReturnThatValue()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var metricType = "temperature";
        var expectedKey = $"metrics:{deviceId}:{metricType}";
        var singleValue = 42.5;

        _databaseMock.Setup(d => d.SortedSetRangeByRankAsync(
            expectedKey,
            It.IsAny<long>(),
            It.IsAny<long>(),
            It.IsAny<Order>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue[] { singleValue });

        // Act
        var result = await _repository.GetAverageAsync(deviceId, metricType);

        // Assert
        result.Should().Be(singleValue);
    }

    [Test]
    public async Task AddMetricAsync_ShouldUseCorrectKeyFormat()
    {
        // Arrange
        var deviceId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        var metricType = "network_bytes";
        var value = 1024.0;

        RedisKey? capturedKey = null;

        _databaseMock.Setup(d => d.SortedSetAddAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<double>(),
            It.IsAny<SortedSetWhen>(),
            It.IsAny<CommandFlags>()))
            .Callback<RedisKey, RedisValue, double, SortedSetWhen, CommandFlags>((key, _, _, _, _) => capturedKey = key)
            .ReturnsAsync(true);

        _databaseMock.Setup(d => d.KeyExpireAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<ExpireWhen>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _databaseMock.Setup(d => d.SortedSetRemoveRangeByScoreAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<Exclude>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(0);

        // Act
        await _repository.AddMetricAsync(deviceId, metricType, value);

        // Assert
        capturedKey.Should().NotBeNull();
        capturedKey.ToString().Should().Be($"metrics:{deviceId}:{metricType}");
    }

    [Test]
    public async Task GetAverageAsync_WithDecimalValues_ShouldCalculateCorrectly()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var metricType = "cpu_usage";
        var values = new RedisValue[] { 33.33, 66.67 };

        _databaseMock.Setup(d => d.SortedSetRangeByRankAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<long>(),
            It.IsAny<long>(),
            It.IsAny<Order>(),
            It.IsAny<CommandFlags>()))
            .ReturnsAsync(values);

        // Act
        var result = await _repository.GetAverageAsync(deviceId, metricType);

        // Assert
        result.Should().Be(50.0);
    }
}
