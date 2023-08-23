using Astrasend.Application.Commands.PaymentProcessing;
using Astrasend.Infrastructure.Np.RabbitMQ;
using Astrasend.Infrastructure.Np.RabbitMQ.Settings;
using Astrasend.Infrastructure.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using ConnectionFactory = RabbitMQ.Client.ConnectionFactory;

namespace Astrasend.Application.Consumers;

/// <summary>
/// Потребитель для события <see cref="PaymentProcessingCommand"/>
/// </summary>
public class InvoicePaymentConsumer : ConsumerBase, IHostedService
{
    /// ctor
    public InvoicePaymentConsumer(
        ConnectionFactory connectionFactory,
        IOptions<RabbitMqSettings> rabbitMqSettings,
        IOptions<InvoicePaymentConsumerSettings> consumerSettings,
        ILogger logger,
        IServiceProvider serviceProvider) 
        : base(connectionFactory, rabbitMqSettings.Value, consumerSettings.Value, logger, serviceProvider)
    {
        try
        {
            var consumer = new AsyncEventingBasicConsumer(Channel);
            consumer.Received += OnEventReceived<PaymentProcessingCommand>;
            Channel.BasicConsume(queue: Queue, autoAck: false, consumer: consumer);
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Error when try subscribe to {nameof(PaymentProcessingCommand)} event.");
            throw;
        }
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }
}