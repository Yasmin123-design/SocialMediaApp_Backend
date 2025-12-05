using NotificationService.Services.NotificationService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Services.RabbitMqListener
{
    public class RabbitMqListenerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMqListenerService> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqListenerService(IServiceProvider serviceProvider, ILogger<RabbitMqListenerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: "notification_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _logger.LogInformation("RabbitMQ Listener initialized and queue declared.");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var data = JsonSerializer.Deserialize<RabbitMessage>(message);
                    if (data == null)
                    {
                        _logger.LogWarning("Received null or invalid message format.");
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    string? notifyMessage = data.Event switch
                    {
                        "PostLiked" => $"Your post was liked by user {data.OwnerName}.",
                        "UnPostLiked" => $"Your post  was unliked by user {data.OwnerName}.",
                        "Follow" => $"User {data.OwnerName} started following you.",
                        "Unfollow" => $"User {data.OwnerName} unfollowed you.",
                        "Comment" => $"User {data.OwnerName} commented on your post.",
                        "UnComment" => $"User {data.OwnerName} remove comment on your post.",

                        _ => null
                    };

                    string? receiverId = data.Event switch
                    {
                        "PostLiked" => data.OwnerId,
                        "Follow" => data.FollowingId,
                        "Unfollow" => data.FollowingId,
                        "Comment" => data.OwnerId,
                        _ => null
                    };

                    if (notifyMessage != null && !string.IsNullOrEmpty(receiverId))
                    {
                        await notificationService.SendNotificationAsync(receiverId, notifyMessage);
                        _logger.LogInformation("Notification sent to {ReceiverId}: {Message}", receiverId, notifyMessage);
                    }
                    else
                    {
                        _logger.LogWarning("No valid notification generated for event: {Event}", data.Event);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing RabbitMQ message.");
                }
            };

            _channel.BasicConsume(
                queue: "notification_queue",
                autoAck: true,
                consumer: consumer
            );

            _logger.LogInformation("RabbitMQ listener started and waiting for messages...");

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

        private class RabbitMessage
        {
            public string Event { get; set; } = string.Empty;
            public int? PostId { get; set; }
            public string? LikerId { get; set; }
            public string? OwnerId { get; set; }
            public string? OwnerName { get; set; }
            public string? FollowerId { get; set; }
            public string? FollowingId { get; set; }
            public string? CommenterId { get; set; }
        }
    }
}


