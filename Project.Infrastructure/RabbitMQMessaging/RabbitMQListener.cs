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
using System.Threading.Channels;

namespace Project.Infrastructure.RabbitMQMessaging
{
    public class RabbitMQListener : BackgroundService
    {
        private static readonly Uri _uri = new Uri("amqps://akmeanzg:TMOCQxQAEWZjfE0Y7wH5v0TN_XTQ9Xfv@mouse.rmq5.cloudamqp.com/akmeanzg");
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<string, UserResponseDTO> _messageQueue;
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMQListener(ConcurrentDictionary<string, UserResponseDTO>  messageQueue, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _messageQueue = messageQueue;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var factory = new ConnectionFactory { Uri = _uri };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(exchange: "sendAuth", type: ExchangeType.Topic);
            var queueDeclareResult = await _channel.QueueDeclareAsync(durable: true, exclusive: false,
    autoDelete: false, arguments: null);

            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);
            var queueName = queueDeclareResult.QueueName;
            await _channel.QueueBindAsync(queue: queueName, exchange: "sendAuth", routingKey: "secretKeySendAuth");

            var consumer = new AsyncEventingBasicConsumer(_channel);
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
            await _channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

            Console.ReadLine();
        }
    }
}
