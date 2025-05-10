using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interface.BaseRepository.Queue;

namespace Queue.Insert;

public class Worker(
    IQueueRepository<Contact> repository,
    ILogger<Worker> logger) : BackgroundService
{
    private readonly IQueueRepository<Contact> _repository = repository;
    private readonly ILogger<Worker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var conn = await factory.CreateConnectionAsync(cancellationToken);
        using var channel = await conn.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(queue: "Contact.Queue.Insert", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);

        Console.WriteLine("Waiting messages...");

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var entity = JsonConvert.DeserializeObject<Contact>(message);

            _logger.LogInformation("{m}", message);

            await _repository.InsertAsync(entity!, cancellationToken);

            await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync("Contact.Queue.Insert", autoAck: false, consumer, cancellationToken);

        while (!cancellationToken.IsCancellationRequested) { }
    }
}
