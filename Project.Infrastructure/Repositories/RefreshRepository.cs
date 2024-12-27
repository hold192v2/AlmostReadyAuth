using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infrastructure.Repositories
{
    public class RefreshRepository : BaseRepository<RefreshSession>, IRefreshRepository
    {
        private readonly AppDbContext _context;

        public RefreshRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<RefreshSession?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            return _context.refreshSessions.FirstOrDefault(x => x.RefreshToken == refreshToken);
        }

        public async Task<RefreshSession?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return _context.refreshSessions.FirstOrDefault(x => x.UserId == userId);
        }
    }
}
