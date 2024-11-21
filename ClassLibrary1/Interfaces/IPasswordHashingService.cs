using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Interfaces
{
    public interface IPasswordHashingService
    {
        string HashPassword(string password);

        bool VerifyHashPassword(string hashedPassword, string providedPassword);
    }
}
