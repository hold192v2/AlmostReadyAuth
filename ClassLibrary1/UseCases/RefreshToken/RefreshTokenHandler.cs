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

namespace Project.Application.UseCases.RefreshToken
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, Response>
    {
        private readonly IUserInterface _userInterface;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRefreshRepository _refreshRepository;
        private readonly IJwtService _jwtService;
        public RefreshTokenHandler(IUserInterface userInterface, IUnitOfWork unitOfWork, IMapper mapper, IRefreshRepository refreshRepository, IJwtService jwtService)
        { 
            _mapper = mapper;
            _userInterface = userInterface;
            _unitOfWork = unitOfWork;
            _refreshRepository = refreshRepository;
            _jwtService = jwtService;
        }
        public async Task<Response> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            RefreshSession? session;
            try
            {
                session = await _refreshRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
                if (session is null)
                {
                    if (session.ExpiresAt <= DateTime.UtcNow)
                        _refreshRepository.Delete(session);
                    return new Response("Unauthorized Access", 401);
                    
                }
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }

            var user = session.User;
            try
            {
                await _unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }
            UserResponseDTO userResponseDTO = _mapper.Map<UserResponseDTO>(user);
            var accessToken = _jwtService.Generate(userResponseDTO);
            var responce = new EndResponse(accessToken, session.RefreshToken, null);
            return new Response("Token Refreshed", 200, responce);
        }
    }
}
