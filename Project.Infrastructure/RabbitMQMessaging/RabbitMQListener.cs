using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using Project.Domain.Interfaces;
using Project.Application.DTOs;
using Microsoft.Extensions.Hosting;

namespace Project.Infrastructure.RabbitMQMessaging
{
    public class RabbitMQListener : BackgroundService
    {
        private static readonly Uri _uri = new Uri("amqps://oqthqqzy:WWOWApSprfKB45g2Uc6ZeT-_W2mckisr@albatross.rmq.cloudamqp.com/oqthqqzy");
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMQListener(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var factory = new ConnectionFactory { Uri = _uri };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: "AuthoriseUserConnection", type: ExchangeType.Topic);
            var queueDeclareResult = await channel.QueueDeclareAsync(durable: true, exclusive: false,
    autoDelete: false, arguments: null);


            var queueName = queueDeclareResult.QueueName;
            await channel.QueueBindAsync(queue: queueName, exchange: "AuthoriseUserConnection", routingKey: "secretKey");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                using (var scope = _scopeFactory.CreateAsyncScope())
                {
                    var debiting = scope.ServiceProvider.GetRequiredService<IUserInterface>();
                    var body = ea.Body.ToArray();
                    var amount = JsonSerializer.Deserialize<UserResponseDTO>(Encoding.UTF8.GetString(body));

                    return Task.CompletedTask;
                }
            };
            await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

            Console.ReadLine();
        }
    }
}
