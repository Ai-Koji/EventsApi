using NotificationApi.Models;
using NotificationApi.Services;

namespace NotificationApi
{
    public class TelegramNotificationSender : INotificationSender
    {
        private readonly HttpClient _http; //HttpClient из DI, используется для сетевых запросов к Telegram API
        private readonly ILogger<TelegramNotificationSender> _logger; //Логгер для записи успешных и ошибочных попыток отправки.
        private readonly string _botToken;
        private readonly string _chatId;
        public TelegramNotificationSender(
        HttpClient http, IConfiguration config,
        ILogger<TelegramNotificationSender> logger)
        {
            _http = http;
            _logger = logger;
            _botToken = config["TelegramApi:BotToken"]
            ?? throw new
            ArgumentNullException("TelegramApi:BotToken");
            _chatId = config["TelegramApi:ChatId"]
            ?? throw new ArgumentNullException("TelegramApi:ChatId");
        }
        public async Task SendAsync(NotificationDto notification)
        {
            var text = $"Напоминание: «{notification.Title}» в {notification.EventDate:u}";
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new //Создание анонимного объекта
            {
                chat_id = _chatId,
                text
            };
            var response = await _http.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
                _logger.LogError(
                "Ошибка при отправке Telegram-сообщения: {Status} {Reason}"
                ,
                response.StatusCode, response.ReasonPhrase);
            else
                _logger.LogInformation("Telegram-сообщение отправлено: {Text}", text);
        }
    }
}