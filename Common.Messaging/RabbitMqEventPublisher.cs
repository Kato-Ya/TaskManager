using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Common.Messaging;

public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly SemaphoreSlim _channelSemaphore = new SemaphoreSlim(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqEventPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task PublishAsync(Guid eventId, string routingKey, string payload, CancellationToken cancellationToken)
    {
        await _channelSemaphore.WaitAsync(cancellationToken);

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.PublishTimeoutSeconds));

            if (_channel == null || !_channel.IsOpen)
            {
                await DisconnectAsync();
                var connectionFactory = _options.CreateConnectionFactory();
                _connection = await connectionFactory.CreateConnectionAsync(timeout.Token);

                var channelOptions = new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true);

                _channel = await _connection.CreateChannelAsync(channelOptions, timeout.Token);
                await RabbitMqTopology.DeclareAsync(_channel, _options, timeout.Token);
            }

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                MessageId = eventId.ToString("D"),
                Type = routingKey
            };

            var body = Encoding.UTF8.GetBytes(payload);

            await _channel.BasicPublishAsync(
                _options.Exchange,
                routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: timeout.Token);
        }
        catch
        {
            await DisconnectAsync();
            throw;
        }
        finally
        {
            _channelSemaphore.Release();
        }
    }

    private async ValueTask DisconnectAsync()
    {
        try
        {
            if (_channel != null)
            {
                await _channel.DisposeAsync();
            }
        }
        finally
        {
            _channel = null;
            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }

            _connection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _channelSemaphore.Dispose();
    }
}