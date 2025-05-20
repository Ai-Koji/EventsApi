namespace NotificationApi.Models
{
    public class NotificationDto
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public DateTime EventDate { get; set; }
        // Пустой конструктор для model binding
        public NotificationDto() { }
        public NotificationDto(int eventId, string title, DateTime eventDate)
        {
            EventId = eventId;
            Title = title;
            EventDate = eventDate;
        }
    }

}
