using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IUserInterface :IBaseOperationRepository<User>
    {
        Task<User?> GetUserByPhoneAsync(string phone, CancellationToken cancellationToken);
        public Task<User?> GetUserByRefreshCode(Guid refreshToken, CancellationToken cancellationToken);
        Task<bool> AnyAsync(string  phone, CancellationToken cancellationToken);
    }
}
