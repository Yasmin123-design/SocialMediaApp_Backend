using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using SearchService.Models;
using System.Text;
using System.Text.Json;
using SearchService.Dtos;

namespace SearchService.Services.RabbitMqListener
{
    public class SearchIndexListener : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public SearchIndexListener(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "search_index_queue",
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                using var scope = _serviceProvider.CreateScope();
                var searchService = scope.ServiceProvider.GetRequiredService<SearchService.SearchService.ISearchService>();

                try
                {
                    var doc = JsonSerializer.Deserialize<SearchIndexDocument>(message);

                    if (doc != null && !string.IsNullOrEmpty(doc.Id))
                    {
                        await searchService.IndexDocumentAsync(doc);
                    }
                    else
                    {
                        var deleteMessage = JsonSerializer.Deserialize<DeleteSearchDocumentMessageDto>(message);
                        if (deleteMessage != null && deleteMessage.Action == "Delete")
                        {
                            await searchService.DeleteDocumentAsync(deleteMessage.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                }
            };

            _channel.BasicConsume(queue: "search_index_queue",
                                  autoAck: true,
                                  consumer: consumer);

            return Task.CompletedTask;
        }



    }
}
