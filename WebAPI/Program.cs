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
using Project.Infrastructure.RabbitMQMessaging;

var builder = WebApplication.CreateBuilder(args);

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



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
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

builder.Services.ConfigureCorsPolicy();
builder.Services.AddHostedService<RabbitMQListener>();

builder.AddJwtAuthentication();

var app = builder.Build();

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

