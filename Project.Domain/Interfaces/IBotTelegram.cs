using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Project.Domain.Interfaces
{
    public interface IBotTelegram : IBaseOperationRepository<BotInputData>
    {
        Task SavePhoneNumberAsync(string phoneNumber, string clientIdentifier);
        Task<BotInputData?> GetByClientIdentifierAsync(string clientIdentifier);
        Task RemoveByClientIdentifierAsync(string clientIdentifier);
    }
}
