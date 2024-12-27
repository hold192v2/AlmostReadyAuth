using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Project.Domain.Security;
using System.Security.Cryptography;
using System.Text;

namespace WebAPI.Extentions
{
    public static class CodeGenerator
    {
        public static int GenerateCode()
        {
            var buffer = new byte[4]; // Для хранения 32-битного числа
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }

            int generatedValue = BitConverter.ToInt32(buffer, 0) & int.MaxValue; // Убираем знак
            return 100000 + (generatedValue % 900000); // Приводим к диапазону 100000-999999
        }
    }
}