using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Common.Messaging;

public class RabbitMqConsumerWorker : BackgroundService
{
    private const string RetryHeader = "x-tmapi-retry-count";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConsumerWorker> _logger;

    public RabbitMqConsumerWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqConsumerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification consumer disconnected; reconnecting");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.RetryDelaySeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        var connectionFactory = _options.CreateConnectionFactory();
        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        await using var forwardingChannel = await connection.CreateChannelAsync(channelOptions, cancellationToken);
        var consumerStopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        channel.ChannelShutdownAsync += (_, _) =>
        {
            consumerStopped.TrySetResult();
            return Task.CompletedTask;
        };

        channel.CallbackExceptionAsync += (_, args) =>
        {
            consumerStopped.TrySetException(args.Exception);
            return Task.CompletedTask;
        };

        await RabbitMqTopology.DeclareAsync(channel, _options, cancellationToken);
        await channel.BasicQosAsync(0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.UnregisteredAsync += (_, _) =>
        {
            consumerStopped.TrySetResult();
            return Task.CompletedTask;
        };

        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                await HandleDeliveryAsync(channel, forwardingChannel, delivery, cancellationToken);
            }
            catch (Exception ex)
            {
                consumerStopped.TrySetException(ex);
            }
        };

        await channel.BasicConsumeAsync(
            _options.NotificationQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Consuming notification events from {Queue}", _options.NotificationQueue);
        await consumerStopped.Task.WaitAsync(cancellationToken);
    }

    private async Task HandleDeliveryAsync(
        IChannel channel,
        IChannel forwardingChannel,
        BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler>();

            await handler.HandleAsync(
                delivery.BasicProperties.Type,
                delivery.BasicProperties.MessageId,
                delivery.Body,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            var retries = ReadRetryCount(delivery.BasicProperties);
            var deadLetter = ex is InvalidIntegrationEventException || retries >= _options.MaxRetries;
            var destination = deadLetter ? _options.DeadLetterQueue : _options.NotificationQueue;

            _logger.LogWarning(ex, "Event {EventId} failed at attempt {Attempt}; forwarding to {Queue}",
                delivery.BasicProperties.MessageId, (long)retries + 1, destination);

            if (!deadLetter)
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.RetryDelaySeconds), cancellationToken);
            }

            var properties = new BasicProperties(delivery.BasicProperties)
            {
                Persistent = true,
                Headers = delivery.BasicProperties.Headers == null
                    ? new Dictionary<string, object?>()
                    : new Dictionary<string, object?>(delivery.BasicProperties.Headers)
            };
            properties.Headers[RetryHeader] = deadLetter ? retries : retries + 1;
            properties.Headers["x-tmapi-error-type"] = ex.GetType().Name;

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.PublishTimeoutSeconds));

            await forwardingChannel.BasicPublishAsync(
                "",
                destination,
                mandatory: true,
                basicProperties: properties,
                body: delivery.Body,
                cancellationToken: timeout.Token);
        }

        await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
    }

    private static int ReadRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers == null || !properties.Headers.TryGetValue(RetryHeader, out var value))
        {
            return 0;
        }

        if (value is int count && count >= 0)
        {
            return count;
        }

        if (value is long longCount && longCount >= 0 && longCount <= int.MaxValue)
        {
            return (int)longCount;
        }

        return int.MaxValue;
    }
}