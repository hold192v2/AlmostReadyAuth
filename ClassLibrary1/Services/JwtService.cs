using Microsoft.IdentityModel.Tokens;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Domain.Security;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Services
{
    /// <summary>
    /// Сервис для работы с JWT токенами.
    /// </summary>
    public class JwtService : IJwtService // Реализация интерфейса для создания методов Generate и GenerateClaims.
    {
        /// <summary>
        /// Генерирует JWT токен на основе предоставленных данных пользователя.
        /// </summary>
        /// <param name="data">Данные пользователя для включения в токен.</param>
        /// <returns>Строка JWT токена.</returns>
        public string Generate(UserResponseDTO data)
        {
            // Создание обработчика для генерации токена.
            var handler = new JwtSecurityTokenHandler();

            // Получение секретного ключа для подписи токена.
            var key = Encoding.ASCII.GetBytes(Configaration.Secrets.JwtPrivateKey);

            // Определение учетных данных для подписи токена.
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            // Описание токена (заголовок, тело и подпись).
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = GenerateClaims(data), // Указание данных пользователя (Claims).
                Expires = DateTime.UtcNow.AddHours(2), // Время жизни токена.
                SigningCredentials = credentials, // Подпись токена.
            };

            // Создание токена.
            var token = handler.CreateToken(tokenDescriptor);

            // Возврат токена в виде строки.
            return handler.WriteToken(token);
        }

        /// <summary>
        /// Создает ClaimsIdentity на основе данных пользователя.
        /// </summary>
        /// <param name="user">Объект, содержащий информацию о пользователе.</param>
        /// <returns>ClaimsIdentity, содержащий данные пользователя.</returns>
        public ClaimsIdentity GenerateClaims(UserResponseDTO user)
        {
            // Создание объекта ClaimsIdentity.
            var claims = new ClaimsIdentity();

            // Добавление основного идентификатора пользователя.
            claims.AddClaim(new Claim("Id", user.Id.ToString()));

            // Добавление имени пользователя (PhoneNumber).
            claims.AddClaim(new Claim(ClaimTypes.Name, user.PhoneNumber));

            // Добавление ролей пользователя.
            foreach (var role in user.Roles)
            {
                claims.AddClaim(new Claim(ClaimTypes.Role, role.Name));
            }

            // Возврат списка утверждений.
            return claims;
        }
    }
}
