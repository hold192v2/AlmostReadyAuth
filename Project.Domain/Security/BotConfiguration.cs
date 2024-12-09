using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Security
{
    public class BotConfiguration
    {

        public static BotSecretsConfiguration Secrets { get; set; } = new();
        public class BotSecretsConfiguration
        {
            public string BotToken { get; set; } = string.Empty;
            public Uri BotWebhookUrl { get; set; } = default!;
            public string SecretToken { get; set; } = string.Empty;
        }
    }
}
