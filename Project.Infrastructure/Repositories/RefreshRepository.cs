using Hangfire;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Project.Infrastructure.Repositories
{
    public class RefreshRepository : BaseRepository<RefreshSession>, IRefreshRepository
    {
        private readonly AppDbContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public RefreshRepository(AppDbContext context, IBackgroundJobClient backgroundJobClient) : base(context)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
        }
        public async Task<RefreshSession?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            return _context.refreshSessions.FirstOrDefault(x => x.RefreshToken == refreshToken);
        }

        public async Task<RefreshSession?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return _context.refreshSessions.FirstOrDefault(x => x.UserId == userId);
        }
        public override void Create(RefreshSession refreshSession)
        {
            refreshSession.DateCreated = DateTimeOffset.UtcNow;
            _context.refreshSessions.Add(refreshSession);

            _backgroundJobClient.Schedule(() => DeleteSession(refreshSession.Id), TimeSpan.FromDays(60));
        }
        public void DeleteSession(Guid sessionId)
        {
            var refreshSession = _context.refreshSessions.Find(sessionId);
            if (refreshSession != null)
            {
                // Устанавливаем DateDeleted в текущее время вместо физического удаления
                Delete(refreshSession);
                _context.SaveChanges();
            }

        }
    }
}
