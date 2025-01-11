using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Project.Domain.Interfaces;
using Project.Domain.Security;
using Project.Application.HadlerResponce;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using static Project.Domain.Security.BotConfiguration;
using Project.Application.DTOs;
using AutoMapper;
using Project.Domain.Entities;
using Telegram.Bot.Requests.Abstractions;
using Project.Application.Interfaces;

namespace Project.Application.UseCases.TelegramBot
{
    public class TelegramBotHandler : IRequestHandler<TelegramBotRequest, Response>
    {
        private readonly IMapper _mapper;
        private readonly IUserInterface _userInterface;
        private readonly ITelegramBotClient _botClient;
        private readonly IBotTelegram _botInputRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<BotSecretsConfiguration> _config;
        private readonly IUpdateHandler _updateHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;
        private readonly IRefreshRepository _refreshRepository;

        public TelegramBotHandler(
            IMapper mapper,
            IUserInterface userInterface,
            ITelegramBotClient botClient,
            IBotTelegram botInputRepository,
            IUnitOfWork unitOfWork,
            IOptions<BotSecretsConfiguration> config,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService,
            IRefreshRepository refreshRepository)
        {
            _mapper = mapper;
            _userInterface = userInterface;
            _botClient = botClient;
            _botInputRepository = botInputRepository;
            _unitOfWork = unitOfWork;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _refreshRepository = refreshRepository;
        }

        public async Task<Response> Handle(TelegramBotRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Проверяем токен
                ValidateSecretToken(request.SecretToken);


                // Если сообщение есть, обрабатываем его
                if (request.Update.Message != null)
                {
                    var response = await ProcessMessageAsync(request.Update.Message, cancellationToken);

                    if (response == null)
                        return new Response("Message processing failed", 400);
                    await _unitOfWork.Commit(cancellationToken); // Сохраняем изменения
                    return response;
                }

                // Обрабатываем любое другое обновление
                await _updateHandler.HandleUpdateAsync(_botClient, request.Update, cancellationToken);
                return new Response("Update processed successfully", 200);
            }
           
            catch (Exception ex)
            {
                return new Response($"Error: {ex.Message}", 500);
            }
        }

        private void ValidateSecretToken(string secretToken)
        {
            if (secretToken != BotConfiguration.Secrets.SecretToken)
            {
                throw new UnauthorizedAccessException($"Invalid secret token. {secretToken} {_config.Value.SecretToken}");
            }
        }

        private async Task<Response?> ProcessMessageAsync(Message message, CancellationToken cancellationToken)
        {
            // Получаем данные по идентификатору клиента
            var replyMarkup = new ReplyKeyboardMarkup(true)
            .AddButton(KeyboardButton.WithRequestContact("Поделиться контактом"));
            var contactNumber = message.Contact?.PhoneNumber;
            var existingData = await _botInputRepository.GetByPhoneAsync(contactNumber);
            
            if (existingData != null && existingData.InputPhone == message.Contact?.PhoneNumber)
            {
                
                // Генерация токена после подтверждения номера
                var user = await _userInterface.GetUserByPhoneAsync(message.Contact.PhoneNumber, cancellationToken);
                if (user == null)
                {
                    await _botClient.SendMessage(
                        message.Chat.Id,
                        "Пользователь не найден. Похоже, вы не являетесь нашим клиентом.",
                        cancellationToken: cancellationToken,
                        replyMarkup: replyMarkup
                    );
                    return null;
                }

                // Отправляем уведомление с кодом авторизации пользователю в Telegram

                await _botClient.SendMessage(
                    message.Chat.Id,
                    $"Ваш код авторизации: {existingData.GenerateCode}",
                    cancellationToken: cancellationToken,
                    replyMarkup: replyMarkup
                );
                return new Response("Message processed successfully", 200);
            }

            // Обработка в случае первого ввода сообщения || ввода неверного номера телефона
            await _botClient.SendMessage(
                message.Chat.Id,
                existingData == null
                    ? "Добрый день!\nНажмите на кнопку поделиться контактом, чтобы мы могли прислать вам код."
                    : "Номер телефона не совпадает. Возможно это не ваш основной телеграмм аккаунт?",
                cancellationToken: cancellationToken,
                replyMarkup: replyMarkup
            );

            return new Response("200", 400);
        }
    }
}
