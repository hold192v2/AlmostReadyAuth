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
using System.Collections.Concurrent;

namespace Project.Infrastructure.RabbitMQMessaging
{
    public class RabbitMQListener : BackgroundService
    {
        private static readonly Uri _uri = new Uri("amqps://oqthqqzy:WWOWApSprfKB45g2Uc6ZeT-_W2mckisr@albatross.rmq.cloudamqp.com/oqthqqzy");
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<string, UserResponseDTO> _messageQueue;

        public RabbitMQListener(ConcurrentDictionary<string, UserResponseDTO>  messageQueue, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _messageQueue = messageQueue;
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

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);
            var queueName = queueDeclareResult.QueueName;
            await channel.QueueBindAsync(queue: queueName, exchange: "AuthoriseUserConnection", routingKey: "secretKeySendAuth");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var userResponseDTO = JsonSerializer.Deserialize<UserResponseDTO>(Encoding.UTF8.GetString(body));
                if (userResponseDTO != null)
                {
                    _messageQueue[userResponseDTO.PhoneNumber] = userResponseDTO;
                }
                return Task.CompletedTask;

            };
            await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

            Console.ReadLine();
        }
    }
}
