using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserInterface
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) : base(context) 
        {
            _context = context;
        }
        //Cуществует ли пользователь
        public Task<bool> AnyAsync(string phone, CancellationToken cancellationToken)
        {
            return _context.Users.AnyAsync(x => x.PhoneNumber == phone, cancellationToken);
        }
        //Выдает пользователя по телефону
        public Task<User?> GetUserByPhoneAsync(string phone, CancellationToken cancellationToken)
        {
            return _context.Users
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.PhoneNumber == phone, cancellationToken);
        }
        //Выдает пользователя по refresh токену
        public Task<User?> GetUserByRefreshCode(Guid refreshToken, CancellationToken cancellationToken)
        {
            return _context.Users
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken : cancellationToken);
        }
    }
}
