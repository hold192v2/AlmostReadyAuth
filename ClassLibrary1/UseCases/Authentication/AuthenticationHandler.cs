using AutoMapper;
using MediatR;
using Project.Application.DTOs;
using Project.Application.HadlerResponce;
using Project.Application.Interfaces;
using Project.Application.Services;
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
        private readonly IJwtService _jwtService;
        private readonly IRefreshRepository _refreshRepository;
        private readonly IBotTelegram _botTelegram;
        public AuthenticationHandler(IUserInterface userInterface, IMapper mapper, IUnitOfWork unitOfWork, IPasswordHashingService service, IJwtService jwtService, IRefreshRepository refreshRepository, IBotTelegram botTelegram)
        {
            _userInterface = userInterface;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _service = service;
            _jwtService = jwtService;
            _refreshRepository = refreshRepository;
            _botTelegram = botTelegram;
            
        }

        public async Task<Response> Handle(AuthenticationRequest request, CancellationToken cancellationToken)
        {
            User? user;
            BotInputData? botInputData;
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
            botInputData = await _botTelegram.GetByPhoneAsync(request.Phone);
            if (request.Code is null)
            {
                bool isVerified = _service.VerifyHashPassword(user.Password, request.Password);
                if (!isVerified)
                {
                    return new Response("Password dont match", 404);
                }
            }
            else if (request.Password is null || botInputData is null)
            {
                if (request.Code != botInputData.GenerateCode)
                    return new Response("You enter wrong code.", 404);
            }
            
            RefreshSession? refreshSession;
            string? refreshToken;
            UserResponseDTO userDTO = _mapper.Map<UserResponseDTO>(user);
            var accessToken = _jwtService.Generate(userDTO);
            try
            {
                refreshSession = await _refreshRepository.GetByUserIdAsync(user.Id, cancellationToken);
                if (refreshSession is null)
                {
                    refreshToken = Guid.NewGuid().ToString();
                    refreshSession = new RefreshSession
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        RefreshToken = refreshToken,
                        Ip = request.Ip,
                        Fingerprint = request.Fingerprint,
                        ExpiresAt = DateTime.UtcNow.AddDays(60)
                    };
                    _refreshRepository.Create(refreshSession);
                }
                refreshToken = refreshSession.RefreshToken;

            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }
            
            var response = new EndResponse(accessToken, refreshToken);
            
            try
            {
                await _unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }
            return new Response("User authenticated", 200, response);
            
        }
        
    }
}
