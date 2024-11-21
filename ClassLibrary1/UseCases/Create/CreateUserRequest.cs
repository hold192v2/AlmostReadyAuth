using MediatR;
using Project.Application.HadlerResponce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.UseCases.Create
{
    public record CreateUserRequest(
        string Phone,
        string Password,
        List<Guid> RoleIds):
        IRequest<Response>;


}
