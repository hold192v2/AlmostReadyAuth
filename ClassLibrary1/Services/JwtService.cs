﻿using Microsoft.IdentityModel.Tokens;
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
    public class JwtService : IJwtService
    {
        public string Generate(UserResponseDTO data)
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
        public ClaimsIdentity GenerateClaims(UserResponseDTO user)
        {
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim("Id", user.Id.ToString()));
            claims.AddClaim(new Claim(ClaimTypes.Name, user.PhoneNumber));
            foreach (var role in user.Roles)
            {
                claims.AddClaim(new Claim(ClaimTypes.Role, role.Name));
            }
            return claims;
        }
    }
}
