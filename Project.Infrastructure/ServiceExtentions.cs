using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Interfaces;
using Project.Infrastructure.Context;
using Project.Application.RabbitMQMessaging;
using Project.Infrastructure.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Project.Application.UseCases.Authentication;

namespace Project.Infrastructure
{
    public static class ServiceExtentions
    {
        public static void ConfigurePresistanceApp(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("postgres");
            IServiceCollection serviceCollection = services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString, x => x.MigrationsAssembly("Project.Infrastructure")), ServiceLifetime.Scoped);
            services.AddHangfire(config =>
        config.UsePostgreSqlStorage(connectionString));

            services.AddScoped<IRabbitPublisher, RabbitMQPublisher>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IBotTelegram, BotInputRepository>();
            services.AddScoped<IRefreshRepository, RefreshRepository>();
            services.AddScoped<AuthenticationHandler>();
            services.AddSingleton<RabbitMQListener>();
            services.AddSingleton<ConcurrentDictionary<string, UserResponseDTO>>();
            services.AddSingleton<ConcurrentDictionary<string, TaskCompletionSource<UserResponseDTO>>>();
            //services.AddScoped<IRabbitMqService, RabbitMqService>();
        }
    }
}
