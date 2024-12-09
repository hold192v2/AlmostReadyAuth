using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Project.Domain.Security;
using System.Runtime.CompilerServices;
using System.Text;

namespace WebAPI.Extentions
{
    public static class BuilderExtension
    {
        public static void AddConfiguration(this WebApplicationBuilder builder)
        {
            Configaration.Secrets.ApiKey = builder.Configuration.GetSection("Secrets").GetValue<string>("ApiKey") ?? string.Empty;
            Configaration.Secrets.JwtPrivateKey = builder.Configuration.GetSection("Secrets").GetValue<string>("JwtPrivateKey") ?? string.Empty;
            Configaration.Secrets.PasswordSaltKey = builder.Configuration.GetSection("Secrets").GetValue<string>("PasswordSaltKey") ?? string.Empty;
            BotConfiguration.Secrets.BotToken = builder.Configuration.GetSection("BotSecrets").GetValue<string>("BotToken") ?? string.Empty;
            BotConfiguration.Secrets.SecretToken = builder.Configuration.GetSection("BotSecrets").GetValue<string>("SecretToken") ?? string.Empty;
            BotConfiguration.Secrets.BotWebhookUrl = builder.Configuration.GetSection("BotSecrets").GetValue<Uri>("BotWebhookUrl") ?? default!;
        }

        public static void AddJwtAuthentication(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configaration.Secrets.JwtPrivateKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            builder.Services.AddAuthorization();
        }
    }
}
