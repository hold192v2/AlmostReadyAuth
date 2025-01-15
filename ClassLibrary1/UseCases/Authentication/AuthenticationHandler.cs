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
        private readonly ConcurrentDictionary<string, TaskCompletionSource<UserResponseDTO>> _messageDictionary;
        
        public AuthenticationHandler(RabbitMQListener rabbitMQListener, ConcurrentDictionary<string, TaskCompletionSource<UserResponseDTO>> messageDictionary, IMapper mapper, IUnitOfWork unitOfWork, IJwtService jwtService, IRefreshRepository refreshRepository, IBotTelegram botTelegram, IRabbitPublisher rabbitPublisher)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _refreshRepository = refreshRepository;
            _botTelegram = botTelegram;
            _messageDictionary = messageDictionary;
            _rabbitPublisher = rabbitPublisher;

            rabbitMQListener.MessageReceived += OnRabbitMQMessageReceived;
        }

        public async Task<Response> Handle(AuthenticationRequest request, CancellationToken cancellationToken)
        {

            await _rabbitPublisher.SendMessage(request.Phone);
            var tcs = new TaskCompletionSource<UserResponseDTO>();
            _messageDictionary[request.Phone] = tcs;

            var delay = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            var completedTask = await Task.WhenAny(tcs.Task, delay);

            if (completedTask == delay)
            {
                _messageDictionary.TryRemove(request.Phone, out _);
                return new Response("Timeout waiting for RabbitMQ response", 408);
            }

            var userResponseDTO = await tcs.Task;
            _messageDictionary.TryRemove(request.Phone, out _);

            BotInputData? botInputData;
            botInputData = await _botTelegram.GetByPhoneAsync(request.Phone);

            if (botInputData is not null)
            {
                if (request.Code != botInputData.GenerateCode)
                    return new Response("You enter wrong code.", 404);
                _botTelegram.Delete(botInputData);
            }
            
            RefreshSession? refreshSession;
            string? refreshToken;
            var accessToken = _jwtService.Generate(userResponseDTO);
            try
            {
                refreshSession = await _refreshRepository.GetByUserIdAsync(userResponseDTO.Id, cancellationToken);
                if (refreshSession is null)
                {
                    refreshToken = Guid.NewGuid().ToString();
                    refreshSession = new RefreshSession
                    {
                        Id = Guid.NewGuid(),
                        UserId = userResponseDTO.Id,
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
            
            var response = new EndResponse(accessToken, refreshToken, userResponseDTO.Id);
            
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
        private void OnRabbitMQMessageReceived(string phone, UserResponseDTO userResponseDTO)
        {
            if (_messageDictionary.TryGetValue(phone, out var tcs))
            {
                tcs.TrySetResult(userResponseDTO);
            }
        }

    }
}
