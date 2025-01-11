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
    public class RabbitMQListener
    {
        private readonly ConnectionFactory _factory;
        private readonly string _queueName = "GetUserQueue";

        public RabbitMQListener()
        {
            _factory = new ConnectionFactory
            {
                Uri = new Uri("amqps://oqthqqzy:WWOWApSprfKB45g2Uc6ZeT-_W2mckisr@albatross.rmq.cloudamqp.com/oqthqqzy")
            };
        }

        public async Task<UserResponseDTO?> GetUserByPhoneAsync(string phoneNumber)
        {
            using var connection = await _factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Создаем очередь для ответа
            var replyQueue = await channel.QueueDeclareAsync(exclusive: true);
            var consumer = new AsyncEventingBasicConsumer(channel);

            var tcs = new TaskCompletionSource<string>();

            consumer.ReceivedAsync += (model, ea) =>
            {
                var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                tcs.SetResult(response);
                return Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: replyQueue.QueueName, autoAck: true, consumer: consumer);

            // Формируем запрос
            var props = new BasicProperties
            {
                ReplyTo = replyQueue.QueueName,
                CorrelationId = Guid.NewGuid().ToString()
            };

            var message = JsonSerializer.Serialize(new { PhoneNumber = phoneNumber });
            var body = Encoding.UTF8.GetBytes(message);

            // Отправляем запрос
            await channel.BasicPublishAsync(
                exchange: "GettingUserInfo",
                mandatory: true,
                routingKey: "secretKey",
                basicProperties: props,
                body: body
                );

            // Ждем ответ
            var responseMessage = await tcs.Task;
            return JsonSerializer.Deserialize<UserResponseDTO>(responseMessage);
        }
    }
}
