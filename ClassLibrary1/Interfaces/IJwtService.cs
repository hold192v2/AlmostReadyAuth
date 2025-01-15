using Project.Application.DTOs;
using ServiceAbonents.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Interfaces
{
    public  interface IJwtService
    {
        string Generate(TransferForAuthDto data);
        ClaimsIdentity GenerateClaims(TransferForAuthDto user);
    }
}
