using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs
{
    public record EndResponse(string AccessToken, string RefreshToken, Guid? userId);
}