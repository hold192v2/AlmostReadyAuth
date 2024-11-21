using AutoMapper;
using MediatR;
using Project.Application.DTOs;
using Project.Application.HadlerResponce;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.UseCases.RefreshToken
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, Response>
    {
        private readonly IUserInterface _userInterface;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public RefreshTokenHandler(IUserInterface userInterface, IUnitOfWork unitOfWork, IMapper mapper)
        { 
            _mapper = mapper;
            _userInterface = userInterface;
            _unitOfWork = unitOfWork;   
        }
        public async Task<Response> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            User? user;
            try
            {
                user = await _userInterface.GetUserByRefreshCode(request.RefreshToken, cancellationToken);
                if (user is null)
                {
                    return new Response("User not found", 404);
                }
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }
            user.GenerateRefreshToken();
            try
            {
                await _unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }
            UserResponseDTO userResponseDTO = _mapper.Map<UserResponseDTO>(user);

            return new Response("Token Refreshed", 200, userResponseDTO);
        }
    }
}
