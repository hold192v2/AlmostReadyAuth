using MediatR;
using Project.Application.HadlerResponce;
using Project.Application.Interfaces;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.UseCases.Create
{
    public class CreateUserHandler : IRequestHandler<CreateUserRequest, Response>
    {
        private readonly IUserInterface _userInterface;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHashingService _service;
        public CreateUserHandler(IUserInterface userInterface, IRoleRepository roleRepository, IUnitOfWork unitOfWork, IPasswordHashingService service) 
        {
            _userInterface = userInterface;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _service = service;

        }
        public async Task<Response> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
            List<Role> roles = [];
            try
            {
                roles = await _roleRepository.GetRoles(new List<string>() { "08341b79-63e3-42eb-888f-e4c4c8ccc425" });
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }
            try
            {
                bool isAvailable = await _userInterface.AnyAsync(request.Phone, cancellationToken);
                if (isAvailable)
                {
                    return new Response("Email already in use", 404);
                }
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }

            User user = new User(request.Phone, _service.HashPassword(request.Password));
            user.Roles = roles;
            try
            {
                _userInterface.Create(user);
                await _unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }

            return new Response("User created", 200);

        }
    }
}
