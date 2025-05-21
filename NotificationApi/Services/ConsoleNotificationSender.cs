using NotificationApi.Models;

namespace NotificationApi.Services
{
    public class ConsoleNotificationSender : INotificationSender
    {
        private readonly ILogger<ConsoleNotificationSender> _logger;
        public ConsoleNotificationSender(ILogger<ConsoleNotificationSender>
        logger)
        => _logger = logger;
        public Task SendAsync(NotificationDto notification)
        {
            _logger.LogInformation
            (
            "[Console] Событие #{EventId}: {Title} в {Date}"
            ,
            notification.EventId, notification.Title, notification.EventDate
            );
            return Task.CompletedTask;
        }
    }
}
