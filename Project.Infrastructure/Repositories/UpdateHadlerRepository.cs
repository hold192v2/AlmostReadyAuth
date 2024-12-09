using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using static Telegram.Bot.TelegramBotClient;

namespace Project.Infrastructure.Repositories
{
    public class UpdateHadlerRepository(ITelegramBotClient bot, ILogger<UpdateHadlerRepository> logger) : IUpdateHandler
    {
        private readonly int Id;

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            logger.LogInformation("HandleError: {Exception}", exception);
            // Cooldown in case of network connection error
            if (exception is RequestException)
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await (update switch
            {
                { Message: { } message } => OnMessage(message),
                { EditedMessage: { } message } => OnMessage(message),

            });
        }

        private async Task OnMessage(Message msg)
        {
            if (msg.Contact is not null)
            {
                await CheckContactAuthorization(msg);
                return;
            }

            if (msg.Text is not { } messageText)
                return;

            logger.LogInformation("Receive message type: {MessageType}", msg.Type);
            Message sentMessage = await (messageText.Split(' ')[0] switch
            {
                "/request" => RequestContact(msg),
                _ => Usage(msg)
            });
            logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.Id);
        }

        async Task<Message> Usage(Message msg)
        {
            const string usage = """
                <b><u>Bot menu</u></b>:
                /request        - request location 
            """;
            return await bot.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
        }

        async Task<Message> RequestContact(Message msg)
        {

            var replyMarkup = new ReplyKeyboardMarkup(true)
                .AddButton(KeyboardButton.WithRequestContact("Поделиться контактом"));
            return await bot.SendMessage(msg.Chat, "Нажмите кнопку \"поделиться контактом\". Кнопка должна появиться под строкой ввода сообщения или в виде квадратной иконки справа от сообщения.", replyMarkup: replyMarkup);
        }

        private async Task CheckContactAuthorization(Message msg)
        {
            // Статический номер для проверки
            const string staticPhoneNumber = "+79058463752"; // Номер должен быть в формате международного стандарта
            var tgPhoneNumber = msg.Contact?.PhoneNumber;
            if (tgPhoneNumber == staticPhoneNumber)
            {

                await bot.SendMessage(msg.Chat, "Авторизация успешна! Ваш номер подтвержден.", replyMarkup: new ReplyKeyboardRemove());
                logger.LogInformation("User authorized: {PhoneNumber}", tgPhoneNumber);

            }
            else
            {
                await bot.SendMessage(msg.Chat, $"Номер телефона не совпадает. \nНа сайте указан номер {staticPhoneNumber}, а Вы подтверждаете с номера {tgPhoneNumber}", replyMarkup: new ReplyKeyboardRemove());
                logger.LogInformation("Authorization failed for number: {PhoneNumber}", tgPhoneNumber);
            }
        }
    }
}
