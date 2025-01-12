using AutoMapper;
using MediatR;
using Project.Application.DTOs;
using Project.Application.HadlerResponce;
using Project.Application.Interfaces;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.UseCases.RefreshToken
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, Response>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRefreshRepository _refreshRepository;
        private readonly IJwtService _jwtService;
        private readonly IRabbitPublisher _rabbitPublisher;
        ConcurrentDictionary<string, UserResponseDTO> _messageDictionary;
        public RefreshTokenHandler(ConcurrentDictionary<string, UserResponseDTO> messageDictionary, IUnitOfWork unitOfWork, IMapper mapper, IRefreshRepository refreshRepository, IJwtService jwtService, IRabbitPublisher rabbitPublisher)
        { 
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _refreshRepository = refreshRepository;
            _jwtService = jwtService;
            _rabbitPublisher = rabbitPublisher;
            _messageDictionary = messageDictionary;
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
                await _rabbitPublisher.SendMessage(session.UserPhone);
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }
            if (!_messageDictionary.TryGetValue(session.UserPhone, out var userResponseDTO))
            {
                return new Response("No user data available from RabbitMQ", 404);
            }
            try
            {
                await _unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }
            var accessToken = _jwtService.Generate(userResponseDTO);
            var responce = new EndResponse(accessToken, session.RefreshToken, null);
            return new Response("Token Refreshed", 200, responce);
        }
    }
}
