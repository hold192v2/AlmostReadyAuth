using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IRefreshRepository : IBaseOperationRepository<RefreshSession>
    {
        Task<RefreshSession?> GetByTokenAsync(string refreshToken, CancellationToken cancellationToken);
    }
}
