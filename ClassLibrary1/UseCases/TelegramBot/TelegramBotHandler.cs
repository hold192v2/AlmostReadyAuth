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

        public TelegramBotHandler(
            IMapper mapper,
            IUserInterface userInterface,
            ITelegramBotClient botClient,
            IBotTelegram botInputRepository,
            IUnitOfWork unitOfWork,
            IOptions<BotSecretsConfiguration> config,
            IUpdateHandler updateHandler,
            IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _userInterface = userInterface;
            _botClient = botClient;
            _botInputRepository = botInputRepository;
            _unitOfWork = unitOfWork;
            _config = config;
            _updateHandler = updateHandler;
            _httpContextAccessor = httpContextAccessor;
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
                    var clientIdentifier = GetClientIdentifier();
                    var response = await ProcessMessageAsync(request.Update.Message, clientIdentifier, cancellationToken);

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

        private string GetClientIdentifier()
        {
            var forwardedHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            return forwardedHeader ?? ipAddress ?? Guid.NewGuid().ToString();
        }

        private async Task<Response?> ProcessMessageAsync(Message message, string clientIdentifier, CancellationToken cancellationToken)
        {
            // Получаем данные по идентификатору клиента
            var existingData = await _botInputRepository.GetByClientIdentifierAsync("::1");
            var replyMarkup = new ReplyKeyboardMarkup(true)
            .AddButton(KeyboardButton.WithRequestContact("Поделиться контактом"));
            if (existingData != null && existingData.InputPhone == message.Contact?.PhoneNumber)
            {
                
                // Генерация токена после подтверждения номера
                var user = await _userInterface.GetUserByPhoneAsync(message.Contact.PhoneNumber, cancellationToken);
                if (user == null)
                {
                    await _botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "Пользователь не найден. Похоже, вы не являетесь нашим клиентом.",
                        cancellationToken: cancellationToken,
                        replyMarkup: replyMarkup
                    );
                    return null;
                }

                // Генерация JWT токена
                var userResponseDTO = _mapper.Map<UserResponseDTO>(user);


                // Удаление данных после успешной аутентификации
                await _botInputRepository.RemoveByClientIdentifierAsync("::1");

                // Отправляем уведомление пользователю в Telegram

                await _botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Авторизация прошла успешна! Можете перейти обратно на сайт.",
                    cancellationToken: cancellationToken,
                    replyMarkup: replyMarkup

                );
                return new Response("Message processed successfully", 200, userResponseDTO);
            }

            // Обработка в случае ошибки
            await _botClient.SendTextMessageAsync(
                message.Chat.Id,
                existingData == null
                    ? "Не удалось найти номер телефона для аутентификации. Пожалуйста, введите свой номер телефона на сайте."
                    : "Номер телефона не совпадает. Пожалуйста, укажите правильный номер.",
                cancellationToken: cancellationToken,
                replyMarkup: replyMarkup
            );

            return new Response("200", 400);
        }
    }
}
