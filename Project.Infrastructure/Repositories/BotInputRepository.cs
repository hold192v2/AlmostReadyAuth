using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Project.Infrastructure.Repositories
{
    public class BotInputRepository : BaseRepository<BotInputData>, IBotTelegram
    {
        private readonly AppDbContext _context;

        public BotInputRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task SavePhoneNumberAsync(string phoneNumber, string clientIdentifier)
        {
            var data = new BotInputData
            {
                Id = Guid.NewGuid(),
                UserIP = clientIdentifier,
                InputPhone = phoneNumber
            };

            await _context.BotInputDatas.AddAsync(data);
            await _context.SaveChangesAsync();
        }

        public Task<BotInputData?> GetByClientIdentifierAsync(string clientIdentifier)
        {
            return _context.BotInputDatas.FirstOrDefaultAsync(x => x.UserIP == clientIdentifier);
        }

        public async Task RemoveByClientIdentifierAsync(string clientIdentifier)
        {
            var entity = await GetByClientIdentifierAsync(clientIdentifier);

            if (entity != null)
            {
                _context.BotInputDatas.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
