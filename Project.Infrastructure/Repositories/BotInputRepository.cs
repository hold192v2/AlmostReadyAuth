using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly IBackgroundJobClient _backgroundJobClient;

        public BotInputRepository(AppDbContext context, IBackgroundJobClient backgroundJobClient) : base(context)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task SavePhoneNumberAsync(string phoneNumber, string generateCode)
        {
            var data = new BotInputData
            {
                Id = Guid.NewGuid(),
                DateCreated = DateTimeOffset.UtcNow,
                DateUpdated = DateTimeOffset.UtcNow,
                InputPhone = phoneNumber,
                GenerateCode = generateCode
            };

            Create(data);
            await _context.SaveChangesAsync();

            _backgroundJobClient.Schedule(() => DeleteAuthCode(data.Id), TimeSpan.FromMinutes(2));
        }

        public Task<BotInputData?> GetByPhoneAsync(string phone)
        {
            return _context.BotInputDatas.FirstOrDefaultAsync(x => x.InputPhone == phone);
        }

        public async Task RemoveByPhoneAsync(string phone)
        {
            var entity = await GetByPhoneAsync(phone);

            if (entity != null)
            {
                _context.BotInputDatas.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public void DeleteAuthCode(Guid authCodeId)
        {
            var botInputData = _context.BotInputDatas.Find(authCodeId);
            if (botInputData != null)
            {
                // Устанавливаем DateDeleted в текущее время вместо физического удаления
                Delete(botInputData);
                _context.SaveChanges();
            }

        }


    }
}
