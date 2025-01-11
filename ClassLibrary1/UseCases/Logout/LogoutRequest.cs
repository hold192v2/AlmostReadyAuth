using MediatR;
using Project.Application.HadlerResponce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.UseCases.Logout
{
    public record LogoutRequest(Guid UserId) :
        IRequest<Response>;
}
