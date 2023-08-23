using Astrasend.Infrastructure.Np.RabbitMQ.Settings;
using RabbitMQ.Client;
using Serilog;

namespace Astrasend.Infrastructure.Np.RabbitMQ
{
    /// <summary>
    /// RabbitMqClient
    /// </summary>
    /// <remarks>Connecting to rabbitMQ and declare and bind queue</remarks>
    public abstract class RabbitMqClientBase : IDisposable
    {
        /// <summary>
        /// <see cref="ILogger"/>
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Queue
        /// </summary>
        protected readonly string Queue;

        /// <summary>
        /// Exchange
        /// </summary>
        protected readonly string Exchange;
        
        /// <summary>
        /// RabbitMq channel
        /// </summary>
        protected IModel? Channel { get; private set; }
        
        private IConnection? _connection;
        private readonly ConnectionFactory _connectionFactory;

        /// <summary>
        /// ctor
        /// </summary>
        protected RabbitMqClientBase(ConnectionFactory connectionFactory, RabbitMqSettings rabbitMqSettings,
            string exchange, string queueKey, ILogger logger)
        {
            _connectionFactory = connectionFactory;
            Logger = logger;

            Queue = $"{rabbitMqSettings.VirtualHost}.{queueKey}";
            Exchange = $"{rabbitMqSettings.VirtualHost}.{exchange}";

            ConnectToRabbitMq(queueKey);
        }

        private void ConnectToRabbitMq(string queueKey)
        {
            try
            {
                if (_connection == null || _connection.IsOpen == false)
                    _connection = _connectionFactory.CreateConnection();

                if (Channel is { IsOpen: true }) 
                    return;
                
                Channel = _connection.CreateModel();
                
                Channel.ExchangeDeclare(Exchange, type: "direct", durable: true, autoDelete: false);
                Channel.QueueDeclare(queue: Queue, durable: false, exclusive: false, autoDelete: false);
                Channel.QueueBind(queue: Queue, exchange: Exchange, routingKey: queueKey);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Cannot connect to rabbitMq");
                throw;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <remarks>Close and dispose channel and connection</remarks>
        public void Dispose()
        {
            try
            {
                Channel?.Close();
                Channel?.Dispose();
                Channel = null;

                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Cannot dispose RabbitMQ channel or connection");
            }
        }
    }
}
