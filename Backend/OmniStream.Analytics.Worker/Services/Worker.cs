using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OmniStream.Analytics.Worker.Configuration;
using OmniStream.Contracts.Data;
using OmniStream.Contracts.Enum;
using OmniStream.Contracts.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OmniStream.Analytics.Worker.Services
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMqSettings _settings;
        private readonly RedisMetricsRepository _repository;

        private IConnection? _connection;
        private IModel? _channel;

        public Worker(ILogger<Worker> logger, IOptions<OmniStreamSettings> settings, RedisMetricsRepository repository)
        {
            _logger = logger;
            _settings = settings.Value.RabbitMQ;
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            InitRabbitMQ();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var envelope = JsonSerializer.Deserialize<MetricEnvelope>(message);

                    if (envelope != null)
                    {
                        await ProcessMessage(envelope);
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(queue: _settings.QueueName, autoAck: false, consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessMessage(MetricEnvelope envelope)
        {
            if (!TryExtractValue(envelope.Payload, out double value))
            {
                _logger.LogWarning($"Skipping non-numeric metric: {envelope.MetricType}");
                return;
            }

            await _repository.AddMetricAsync(envelope.DeviceId, envelope.MetricType, value);

            var avg = await _repository.GetAverageAsync(envelope.DeviceId, envelope.MetricType);

            _logger.LogInformation($"Device {envelope.DeviceId} | {envelope.MetricType}: {value} | Avg(5min): {avg:F2}");

            if (value > 80.0)
            {
                PublishAlert(envelope, value, avg);
            }
        }

        private void PublishAlert(MetricEnvelope source, double current, double avg)
        {
            _logger.LogWarning("!!! ANOMALY DETECTED !!! Sending alert...");

            var alert = new AnomalyDetected
            {
                DeviceId = source.DeviceId,
                MetricType = source.MetricType,
                Value = current,
                Message = $"Threshold exceeded! Value: {current}, Avg: {avg:F2}",
                Severity = AlertSeverity.Critical
            };

            var json = JsonSerializer.Serialize(alert);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: _settings.ExchangeName, routingKey: "", basicProperties: null, body: body);
        }

        private bool TryExtractValue(object payload, out double result)
        {
            result = 0;
            if (payload is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Number)
                {
                    result = element.GetDouble();
                    return true;
                }
                if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("value", out var valProp))
                {
                    if (valProp.ValueKind == JsonValueKind.Number)
                    {
                        result = valProp.GetDouble();
                        return true;
                    }
                }
            }
            return false;
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                UserName = _settings.Username,
                Password = _settings.Password,
                DispatchConsumersAsync = true
            };

            var attempts = 0;
            while (true)
            {
                try
                {
                    attempts++;
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _logger.LogInformation("Successfully connected to RabbitMQ after {Attempts} attempts.", attempts);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to connect to RabbitMQ (Attempt {Attempts}). Retrying in 3s... Error: {Error}", attempts, ex.Message);

                    Thread.Sleep(3000);
                }
            }

            _channel.QueueDeclare(queue: _settings.QueueName, durable: false, exclusive: false, autoDelete: false);
            _channel.ExchangeDeclare(exchange: _settings.ExchangeName, type: ExchangeType.Fanout);
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
