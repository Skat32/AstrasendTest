using System.Text;
using Astrasend.Infrastructure.Np.RabbitMQ.Settings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace Astrasend.Infrastructure.Np.RabbitMQ
{
    /// <summary>
    /// Базовай класс потребителя событий
    /// </summary>
    public abstract class ConsumerBase : RabbitMqClientBase
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connectionFactory"><see cref="ConnectionFactory"/></param>
        /// <param name="rabbitMqSettings"><see cref="RabbitMqSettings"/></param>
        /// <param name="rabbitQueueSettings"><see cref="BaseRabbitQueueSettings"/></param>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/></param>
        protected ConsumerBase(
            ConnectionFactory connectionFactory, 
            RabbitMqSettings rabbitMqSettings,
            BaseRabbitQueueSettings rabbitQueueSettings,
            ILogger logger,
            IServiceProvider serviceProvider) 
            : base(
                connectionFactory,
                rabbitMqSettings, 
                rabbitQueueSettings.Exchange,
                rabbitQueueSettings.Queue,
                logger)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Event handler with generic type
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="event">Event with body</param>
        /// <typeparam name="T">Event type</typeparam>
        protected async Task OnEventReceived<T>(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var body = Encoding.UTF8.GetString(@event.Body.ToArray());
                var message = JsonConvert.DeserializeObject<T>(body);

                if (message is null)
                    throw new ArgumentNullException(nameof(message), 
                        $"Message is null. Event - {typeof(T)}. Body - {body}");

                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while retrieving message from queue.");
            }
            finally
            {
                Channel?.BasicAck(@event.DeliveryTag, false);
            }
        }
    }
}
