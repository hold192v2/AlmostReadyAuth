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
using MassTransit;
using ServiceAbonents.Dtos;

namespace Project.Application.RabbitMQMessaging
{
    public class RabbitMQListener : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<string, TransferForAuthDto> _messageQueue;
        private readonly IBusControl _bus;

        public RabbitMQListener(ConcurrentDictionary<string, TransferForAuthDto> messageQueue, IServiceScopeFactory scopeFactory, IBusControl bus)
        {
            _scopeFactory = scopeFactory;
            _messageQueue = messageQueue;
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        }
    }
}
