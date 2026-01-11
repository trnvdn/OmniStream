using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using OmniStream.Analytics.Worker.Configuration;

namespace OmniStream.Analytics.Worker.Tests.Configuration;

[TestFixture]
public class ConfigurationTests
{
    [TestFixture]
    public class RedisSettingsTests
    {
        [Test]
        public void RedisSettings_WithValidConnectionString_ShouldPassValidation()
        {
            // Arrange
            var settings = new RedisSettings
            {
                ConnectionString = "localhost:6379",
                WindowSeconds = 300,
                TtlMinutes = 10
            };

            // Act
            var validationResults = ValidateModel(settings);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Test]
        public void RedisSettings_WithEmptyConnectionString_ShouldFailValidation()
        {
            // Arrange
            var settings = new RedisSettings
            {
                ConnectionString = "",
                WindowSeconds = 300,
                TtlMinutes = 10
            };

            // Act
            var validationResults = ValidateModel(settings);

            // Assert
            validationResults.Should().ContainSingle()
                .Which.MemberNames.Should().Contain(nameof(RedisSettings.ConnectionString));
        }

        [Test]
        public void RedisSettings_WithNullConnectionString_ShouldFailValidation()
        {
            // Arrange
            var settings = new RedisSettings
            {
                ConnectionString = null!,
                WindowSeconds = 300,
                TtlMinutes = 10
            };

            // Act
            var validationResults = ValidateModel(settings);

            // Assert
            validationResults.Should().ContainSingle()
                .Which.MemberNames.Should().Contain(nameof(RedisSettings.ConnectionString));
        }

        [Test]
        public void RedisSettings_DefaultValues_ShouldBeDefault()
        {
            // Arrange & Act
            var settings = new RedisSettings();

            // Assert
            settings.WindowSeconds.Should().Be(default);
            settings.TtlMinutes.Should().Be(default);
        }
    }

    [TestFixture]
    public class RabbitMqSettingsTests
    {
        [Test]
        public void RabbitMqSettings_WithAllRequiredFields_ShouldPassValidation()
        {
            // Arrange
            var settings = new RabbitMqSettings
            {
                Host = "localhost",
                Username = "guest",
                Password = "guest",
                QueueName = "metrics-queue",
                ExchangeName = "alerts-exchange"
            };

            // Act
            var validationResults = ValidateModel(settings);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Test]
        public void RabbitMqSettings_WithMissingHost_ShouldFailValidation()
        {
            // Arrange
            var settings = new RabbitMqSettings
            {
                Host = "",
                Username = "guest",
                Password = "guest",
                QueueName = "metrics-queue",
                ExchangeName = "alerts-exchange"
            };

            // Act
            var validationResults = ValidateModel(settings);

            // Assert
            validationResults.Should().ContainSingle()
                .Which.MemberNames.Should().Contain(nameof(RabbitMqSettings.Host));
        }

        [Test]
        public void RabbitMqSettings_WithMissingUsername_ShouldFailValidation()
        {
            // Arrange
            var settings = new RabbitMqSettings
            {
                Host = "localhost",
                Username = "",
                Password = "guest",
                QueueName = "metrics-queue",
                ExchangeName = "alerts-exchange"
            };

            // Act
            var validationResults = ValidateModel(settings);

            // Assert
            validationResults.Should().ContainSingle()
                .Which.MemberNames.Should().Contain(nameof(RabbitMqSettings.Username));
        }

        [Test]
        public void RabbitMqSettings_WithMissingPassword_ShouldFailValidation()
        {
            // Arrange
            var settings = new RabbitMqSettings
            {
                Host = "localhost",
                Username = "guest",
                Password = "",
                QueueName = "metrics-queue",
                ExchangeName = "alerts-exchange"
            };

            // Act
            var validationResults = ValidateModel(settings);

            // Assert
            validationResults.Should().ContainSingle()
                .Which.MemberNames.Should().Contain(nameof(RabbitMqSettings.Password));
        }

        [Test]
        public void RabbitMqSettings_WithMissingQueueName_ShouldFailValidation()
        {
            // Arrange
            var settings = new RabbitMqSettings
            {
                Host = "localhost",
                Username = "guest",
                Password = "guest",
                QueueName = "",
                ExchangeName = "alerts-exchange"
            };

            // Act
            var validationResults = ValidateModel(settings);

            // Assert
            validationResults.Should().ContainSingle()
                .Which.MemberNames.Should().Contain(nameof(RabbitMqSettings.QueueName));
        }

        [Test]
        public void RabbitMqSettings_WithMissingExchangeName_ShouldFailValidation()
        {
            // Arrange
            var settings = new RabbitMqSettings
            {
                Host = "localhost",
                Username = "guest",
                Password = "guest",
                QueueName = "metrics-queue",
                ExchangeName = ""
            };

            // Act
            var validationResults = ValidateModel(settings);

            // Assert
            validationResults.Should().ContainSingle()
                .Which.MemberNames.Should().Contain(nameof(RabbitMqSettings.ExchangeName));
        }

        [Test]
        public void RabbitMqSettings_WithMultipleMissingFields_ShouldFailWithMultipleErrors()
        {
            // Arrange
            var settings = new RabbitMqSettings
            {
                Host = "",
                Username = "",
                Password = "guest",
                QueueName = "queue",
                ExchangeName = "exchange"
            };

            // Act
            var validationResults = ValidateModel(settings);

            // Assert
            validationResults.Should().HaveCount(2);
        }
    }

    [TestFixture]
    public class OmniStreamSettingsTests
    {
        [Test]
        public void OmniStreamSettings_DefaultValues_ShouldHaveInitializedChildren()
        {
            // Arrange & Act
            var settings = new OmniStreamSettings();

            // Assert
            settings.RabbitMQ.Should().NotBeNull();
            settings.Redis.Should().NotBeNull();
        }

        [Test]
        public void OmniStreamSettings_CanSetRabbitMqSettings()
        {
            // Arrange
            var rabbitSettings = new RabbitMqSettings
            {
                Host = "rabbitmq.example.com",
                Username = "admin",
                Password = "secret",
                QueueName = "test-queue",
                ExchangeName = "test-exchange"
            };

            // Act
            var settings = new OmniStreamSettings
            {
                RabbitMQ = rabbitSettings
            };

            // Assert
            settings.RabbitMQ.Should().BeSameAs(rabbitSettings);
            settings.RabbitMQ.Host.Should().Be("rabbitmq.example.com");
        }

        [Test]
        public void OmniStreamSettings_CanSetRedisSettings()
        {
            // Arrange
            var redisSettings = new RedisSettings
            {
                ConnectionString = "redis.example.com:6379",
                WindowSeconds = 600,
                TtlMinutes = 15
            };

            // Act
            var settings = new OmniStreamSettings
            {
                Redis = redisSettings
            };

            // Assert
            settings.Redis.Should().BeSameAs(redisSettings);
            settings.Redis.ConnectionString.Should().Be("redis.example.com:6379");
            settings.Redis.WindowSeconds.Should().Be(600);
            settings.Redis.TtlMinutes.Should().Be(15);
        }
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }
}
