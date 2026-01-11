using System.Text.Json;
using FluentAssertions;

namespace OmniStream.Analytics.Worker.Tests.Services;

[TestFixture]
public class WorkerTests
{
    [TestFixture]
    public class TryExtractValueTests
    {
        [Test]
        public void TryExtractValue_WithJsonElementNumber_ShouldReturnTrue()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>("42.5");

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().Be(42.5);
        }

        [Test]
        public void TryExtractValue_WithJsonElementInteger_ShouldReturnTrue()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>("100");

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().Be(100.0);
        }

        [Test]
        public void TryExtractValue_WithJsonElementNegativeNumber_ShouldReturnTrue()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>("-25.75");

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().Be(-25.75);
        }

        [Test]
        public void TryExtractValue_WithJsonObjectContainingValue_ShouldReturnTrue()
        {
            // Arrange
            var json = """{"value": 75.5}""";
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().Be(75.5);
        }

        [Test]
        public void TryExtractValue_WithJsonObjectContainingIntegerValue_ShouldReturnTrue()
        {
            // Arrange
            var json = """{"value": 50}""";
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().Be(50.0);
        }

        [Test]
        public void TryExtractValue_WithJsonObjectWithoutValue_ShouldReturnFalse()
        {
            // Arrange
            var json = """{"data": 75.5}""";
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(0);
        }

        [Test]
        public void TryExtractValue_WithJsonString_ShouldReturnFalse()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>("\"not a number\"");

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(0);
        }

        [Test]
        public void TryExtractValue_WithJsonBoolean_ShouldReturnFalse()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>("true");

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(0);
        }

        [Test]
        public void TryExtractValue_WithJsonNull_ShouldReturnFalse()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>("null");

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(0);
        }

        [Test]
        public void TryExtractValue_WithJsonArray_ShouldReturnFalse()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>("[1, 2, 3]");

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(0);
        }

        [Test]
        public void TryExtractValue_WithJsonObjectValueAsString_ShouldReturnFalse()
        {
            // Arrange
            var json = """{"value": "not a number"}""";
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(0);
        }

        [Test]
        public void TryExtractValue_WithNonJsonElement_ShouldReturnFalse()
        {
            // Arrange
            object payload = "just a string";

            // Act
            var result = Worker.Services.Worker.TryExtractValue(payload, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(0);
        }

        [Test]
        public void TryExtractValue_WithPlainDouble_ShouldReturnFalse()
        {
            // Arrange
            object payload = 42.5;

            // Act
            var result = Worker.Services.Worker.TryExtractValue(payload, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(0);
        }

        [Test]
        public void TryExtractValue_WithZero_ShouldReturnTrue()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>("0");

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().Be(0);
        }

        [Test]
        public void TryExtractValue_WithVeryLargeNumber_ShouldReturnTrue()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>("9999999999.99");

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().Be(9999999999.99);
        }

        [Test]
        public void TryExtractValue_WithVerySmallNumber_ShouldReturnTrue()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>("0.0001");

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().Be(0.0001);
        }

        [Test]
        public void TryExtractValue_WithNestedObjectValue_ShouldReturnTrue()
        {
            // Arrange
            var json = """{"id": "test", "value": 85.5, "timestamp": "2024-01-01"}""";
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().Be(85.5);
        }

        [Test]
        public void TryExtractValue_WithEmptyObject_ShouldReturnFalse()
        {
            // Arrange
            var jsonElement = JsonSerializer.Deserialize<JsonElement>("{}");

            // Act
            var result = Worker.Services.Worker.TryExtractValue(jsonElement, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(0);
        }
    }
}
