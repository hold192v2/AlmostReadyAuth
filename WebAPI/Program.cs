using Project.Infrastructure;
using Project.Application.Configuration;
using System.Security.Cryptography.Xml;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using WebAPI.Extentions;
using Project.Infrastructure.Context;
using Telegram.Bot;
using Project.Domain.Security;
using Microsoft.AspNetCore;
using Project.Domain.Interfaces;
using Project.Infrastructure.Repositories;
using Project.Application.Interfaces;
using Project.Application.Services;
using Hangfire;
using Project.Application.RabbitMQMessaging;
using MassTransit;
using Project.Application.DTOs;
using ServiceAbonents.Dtos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:5173")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
// Add services to the container.
builder.AddConfiguration();
builder.Services.AddHangfireServer();
builder.Services.AddHttpClient("tgwebhook").RemoveAllLoggers().AddTypedClient<ITelegramBotClient>(
    httpClient => new TelegramBotClient(BotConfiguration.Secrets.BotToken, httpClient));
builder.Services.ConfigureTelegramBotMvc();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddControllers();
builder.Services.ConfigurePresistanceApp(builder.Configuration);

builder.Services.ConfigureApplicationApp();
builder.Services.AddMvc()
                .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();// Регистрация RequestClient
    x.AddRequestClient<TransferForAuthRequestDTO>();
    // Настройка подключения к шине
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("amqps://akmeanzg:TMOCQxQAEWZjfE0Y7wH5v0TN_XTQ9Xfv@mouse.rmq5.cloudamqp.com/akmeanzg");
        cfg.Message<TransferForAuthRequestDTO>(x => x.SetEntityName("queue-name"));

        cfg.ClearSerialization();
        cfg.UseRawJsonSerializer();
    });

});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });

    options.CustomSchemaIds(type => type.ToString());
});

builder.Services.AddHostedService<RabbitMQListener>();

builder.AddJwtAuthentication();
builder.Services.AddSwaggerGen(options =>
{
    var basePath = AppContext.BaseDirectory;

    var xmlPath = Path.Combine(basePath, "WebAPI.xml");
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.yaml", "v1");
    });
}
CreateDatabase(app);

app.UseCors();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void CreateDatabase(WebApplication app)
{
    var serviceScope = app.Services.CreateScope();
    var dataContext = serviceScope.ServiceProvider.GetService<AppDbContext>();
    dataContext?.Database.EnsureCreated();
}

