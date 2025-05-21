using NotificationApi.Models;

namespace NotificationApi.Services
{
    public interface INotificationSender
    {
        Task SendAsync(NotificationDto notification);
    }
}
