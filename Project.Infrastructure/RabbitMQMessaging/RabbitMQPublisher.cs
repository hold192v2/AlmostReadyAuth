﻿using System.Text.Json;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infrastructure.RabbitMQMessaging
{
    public class RabbitMQPublisher : IRabbitPublisher
    {
        private readonly Uri _uri = new Uri("amqps://akmeanzg:TMOCQxQAEWZjfE0Y7wH5v0TN_XTQ9Xfv@mouse.rmq5.cloudamqp.com/akmeanzg");

        public void SendMessage(object obj)
        {
            var message = JsonSerializer.Serialize(obj);
            SendMessage(message);
        }

        public async Task<bool> SendMessage(UserResponseDTO transaction)
        {
            var factory = new ConnectionFactory() { Uri = _uri };
            using var connection = await factory.CreateConnectionAsync();
            var channelOpts = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true);
            using var channel = await connection.CreateChannelAsync(channelOpts);

            var properties = new BasicProperties
            {
                Persistent = true
            };

            await channel.ExchangeDeclareAsync(exchange: "UserAuthConnection", type: ExchangeType.Topic);
            var routingKey = "secretKeyTransfer";

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(transaction));

            try
            {
                await channel.BasicPublishAsync(exchange: "UserAuthConnection", routingKey: routingKey, body: body);
                Console.WriteLine($"[x] sent {transaction}");
                return true;
            }

            catch
            {
                Console.WriteLine("Message was not sent");
                return false;
            }
        }
    }
}
