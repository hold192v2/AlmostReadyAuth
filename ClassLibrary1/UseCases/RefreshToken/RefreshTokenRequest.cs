﻿using MediatR;
using Project.Application.HadlerResponce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.UseCases.RefreshToken
{
    public record RefreshTokenRequest(string RefreshToken):
        IRequest<Response>;
}
