using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Domain.Interfaces;
using Project.Infrastructure.Context;
using Project.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;

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

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserInterface, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IBotTelegram, BotInputRepository>();
            services.AddScoped<IRefreshRepository, RefreshRepository>();
            //services.AddScoped<IRabbitMqService, RabbitMqService>();
        }
    }
}
