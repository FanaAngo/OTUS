using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ProjectAfishaBot
{
    class TelegramCore
    {
        public TelegramBotClient Token()
        {
            string readKey = System.IO.File.ReadAllText(@"путь к файлу\Telegtam-token.txt");
            return new TelegramBotClient(readKey);
        }

        public HttpClient HttpClientMethod()
        {
            string readKey = System.IO.File.ReadAllText(@"путь к файлу\API-key.txt");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", readKey);
            return client;
        }

        public ReplyKeyboardMarkup Buttons()
        {
            return new(new[]
                            {
                    new KeyboardButton[] { "Фильмы в прокате", "Премьеры" },
                })
            {
                ResizeKeyboard = true
            };
        }

        public void InputMessage(Update update, out string author, out string greetings, out string sellectMessage, out string soryMessage, out string nowPlayingLink, out string upcomingLink)
        {
            author = $"Автор бота: Гаврильев П.П.\nПроектная работа по курсу: C# Developer. Basic\nШкола: ООО «Отус онлайн-образование»\nВерсия: 1.1 от 24.01.2022";
            greetings = "Я Вас приветствую, мастер " + update.Message.From.FirstName + ".";
            sellectMessage = "Какую категорию Вы предпочтете <Фильмы в прокате> или <Премьеры>?";
            soryMessage = "Извините, в ответах я ограничен. Правильно задавайте команды.";
            nowPlayingLink = "https://api.themoviedb.org/3/movie/now_playing?language=ru";
            upcomingLink = "https://api.themoviedb.org/3/movie/upcoming?language=ru";
        }

        public async Task TextMessage(ITelegramBotClient botClient, long chatId, ReplyKeyboardMarkup replyKeyboardMarkup, CancellationToken cancellationToken, string inputText)
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: inputText,
                                                replyMarkup: replyKeyboardMarkup,
                                                cancellationToken: cancellationToken);
        }

        public async Task MovieMessage(ITelegramBotClient botClient, long chatId, TelegramCore start, string sellectMessage, ReplyKeyboardMarkup replyKeyboardMarkup, HttpClient client, CancellationToken cancellationToken, string link)
        {
            try
            {
                var response = await client.GetAsync(link);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<MovieDetailsResponse>(responseContent);
                foreach (var movies in data.Movie)
                {
                    Message sentMovieNowPlaying = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"Название фильма: {movies.Title}\n" +
                            $"Премьера: {movies.ReleaseDate}\n" +
                            $"Рейтинг: {movies.voteAverage}\n" +
                            $"Ссылка на фильм: https://www.themoviedb.org/movie/{movies.ID}?language=ru",
                            replyMarkup: replyKeyboardMarkup,
                            cancellationToken: cancellationToken);
                }
                await start.TextMessage(botClient, chatId, replyKeyboardMarkup, cancellationToken, sellectMessage);
                client.Dispose();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка получения данных: {ex.Message}");
            }
        }
    }
}
