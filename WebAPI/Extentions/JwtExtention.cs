using Microsoft.IdentityModel.Tokens;
using Project.Application.DTOs;
using Project.Domain.Entities;
using Project.Domain.Security;
using ServiceAbonents.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebAPI.Extentions
{
    public static class JwtExtention
    {
        public static string Generate(TransferForAuthDto data)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Configaration.Secrets.JwtPrivateKey);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = GenerateClaims(data),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = credentials,
            };
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
        private static ClaimsIdentity GenerateClaims(TransferForAuthDto user)
        {
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim("Id", user.AbonentId.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.Name, user.PhoneNumber));
            claims.AddClaim(new Claim(ClaimTypes.Role, user.Role));
            return claims;
        }
    }
}
