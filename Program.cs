using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ProjectAfishaBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var start = new TelegramCore();
            TelegramBotClient botClient = start.Token();

            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };
            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);

            var me = await botClient.GetMeAsync(); // требуется интернет

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            //Send cancellation request to stop bot
            cts.Cancel();

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                // Only process Message updates: https://core.telegram.org/bots/api#message
                if (update.Type != UpdateType.Message)
                    return;
                // Only process text messages
                if (update.Message!.Type != MessageType.Text)
                    return;

                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;

                Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

                ReplyKeyboardMarkup replyKeyboardMarkup = start.Buttons();
                string author, greetings, sellectMessage, soryMessage, nowPlayingLink, upcomingLink;
                start.InputMessage(update, out author, out greetings, out sellectMessage, out soryMessage, out nowPlayingLink, out upcomingLink);

                if (messageText == "/start")
                {
                    await start.TextMessage(botClient, chatId, replyKeyboardMarkup, cancellationToken, greetings);
                    await start.TextMessage(botClient, chatId, replyKeyboardMarkup, cancellationToken, sellectMessage);
                }

                HttpClient client = start.HttpClientMethod();

                switch (messageText)
                {
                    case "Фильмы в прокате":
                        {
                            await start.MovieMessage(botClient, chatId, start, sellectMessage, replyKeyboardMarkup, client, cancellationToken, nowPlayingLink);
                            break;
                        }
                    case "Премьеры":
                        {
                            await start.MovieMessage(botClient, chatId, start, sellectMessage, replyKeyboardMarkup, client, cancellationToken, upcomingLink);
                            break;
                        }
                    case "Автор":
                        {
                            await start.TextMessage(botClient, chatId, replyKeyboardMarkup, cancellationToken, author);
                            break;
                        }
                }

                if (messageText != "Фильмы в прокате" && messageText != "Премьеры" && messageText != "/start" && messageText != "Автор")
                {
                    await start.TextMessage(botClient, chatId, replyKeyboardMarkup, cancellationToken, soryMessage);
                }
            }

            Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
        }
    }
}