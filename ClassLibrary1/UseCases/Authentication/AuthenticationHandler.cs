using AutoMapper;
using MediatR;
using Project.Application.DTOs;
using Project.Application.HadlerResponce;
using Project.Application.Interfaces;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.UseCases.Authentication
{
    public class AuthenticationHandler : IRequestHandler<AuthenticationRequest, Response>
    {
        private readonly IUserInterface _userInterface;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHashingService _service;
        public AuthenticationHandler(IUserInterface userInterface, IMapper mapper, IUnitOfWork unitOfWork, IPasswordHashingService service)
        {
            _userInterface = userInterface;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _service = service;
        }

        public async Task<Response> Handle(AuthenticationRequest request, CancellationToken cancellationToken)
        {
            User? user;
            try
            {
                user = await _userInterface.GetUserByPhoneAsync(request.Phone, cancellationToken);
                if (user is null)
                    return new Response("User not found", 404);

            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }

            bool isVerified = _service.VerifyHashPassword(user.Password, request.Password);
            if (!isVerified)
            {
                return new Response("Password dont match", 404);
            }
            try
            {
                await _unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }
            UserResponseDTO userDTO = _mapper.Map<UserResponseDTO>(user);
            return new Response("User authenticated", 200, userDTO);
        }
        
    }
}
