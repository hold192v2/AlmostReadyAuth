using AutoMapper;
using MediatR;
using Project.Application.DTOs;
using Project.Application.HadlerResponce;
using Project.Application.Interfaces;
using Project.Application.Services;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Project.Application.RabbitMQMessaging;
using MassTransit.Clients;
using MassTransit;
using Response = Project.Application.HadlerResponce.Response;
using ServiceAbonents.Dtos;

namespace Project.Application.UseCases.Authentication
{
    public class AuthenticationHandler : IRequestHandler<AuthenticationRequest, Response>
    {
        //private readonly RabbitMQListener;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IRefreshRepository _refreshRepository;
        private readonly IBotTelegram _botTelegram;
        private readonly IRabbitPublisher _rabbitPublisher;
        private readonly IRequestClient<TransferForAuthRequestDTO> _client;
        
        public AuthenticationHandler(IRequestClient<TransferForAuthRequestDTO> client,RabbitMQListener rabbitMQListener, IMapper mapper, IUnitOfWork unitOfWork, IJwtService jwtService, IRefreshRepository refreshRepository, IBotTelegram botTelegram, IRabbitPublisher rabbitPublisher)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _refreshRepository = refreshRepository;
            _botTelegram = botTelegram;
            _rabbitPublisher = rabbitPublisher;
            _client = client;
        }

        public async Task<Response> Handle(AuthenticationRequest request, CancellationToken cancellationToken)
        {
            var response = await _client.GetResponse<TransferForAuthDto>(new TransferForAuthRequestDTO() { PhoneNumber = request.Phone }); 

            var userResponseDTO = response.Message;

            BotInputData? botInputData;
            botInputData = await _botTelegram.GetByPhoneAsync(request.Phone);

            if (botInputData is not null)
            {
                if (request.Code != botInputData.GenerateCode)
                    return new Response("You enter wrong code.", 404);
                _botTelegram.Delete(botInputData);
            }
            else
            {
                return new Response("Вы не вводили номер телефона!", 404);
            }
            
            RefreshSession? refreshSession;
            string? refreshToken;
            var accessToken = _jwtService.Generate(userResponseDTO);
            try
            {
                refreshSession = await _refreshRepository.GetByUserIdAsync(Guid.Parse(userResponseDTO.AbonentId), cancellationToken);
                if (refreshSession is null)
                {
                    refreshToken = Guid.NewGuid().ToString();
                    refreshSession = new RefreshSession
                    {
                        Id = Guid.NewGuid(),
                        UserId = Guid.Parse(userResponseDTO.AbonentId),
                        UserPhone = userResponseDTO.PhoneNumber,
                        RefreshToken = refreshToken,
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
            
            var endResponse = new EndResponse(accessToken, refreshToken, Guid.Parse(userResponseDTO.AbonentId));
            
            try
            {
                await _unitOfWork.Commit(cancellationToken);
            }
            catch
            {
                return new Response("Internal Server Error", 500);
            }
            return new Response("User authenticated", 200, endResponse);
            
        }

    }
}
