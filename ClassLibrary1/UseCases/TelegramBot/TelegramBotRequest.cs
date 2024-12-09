using MediatR;
using Project.Application.HadlerResponce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Project.Application.UseCases.TelegramBot
{
    public record TelegramBotRequest(Update Update, string SecretToken, CancellationToken CancellationToken) : IRequest<Response>;
}
