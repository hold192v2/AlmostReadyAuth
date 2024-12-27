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
            var buffer = new byte[4]; // ��� �������� 32-������� �����
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }

            int generatedValue = BitConverter.ToInt32(buffer, 0) & int.MaxValue; // ������� ����
            return 100000 + (generatedValue % 900000); // �������� � ��������� 100000-999999
        }
    }
}