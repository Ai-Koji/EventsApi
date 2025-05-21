using NotificationApi.Services;
using NotificationApi;
using NotificationApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true).AddEnvironmentVariables();
builder.Services.AddSingleton<INotificationSender, ConsoleNotificationSender>();
builder.Services.AddHttpClient<TelegramNotificationSender>();
builder.Services.AddSingleton<INotificationSender>(sp =>

sp.GetRequiredService<TelegramNotificationSender>());
var app = builder.Build();

app.MapPost("/api/notifications"
, async (NotificationDto notification,
IEnumerable<INotificationSender> senders) =>
{
    var tasks = senders.Select(s => s.SendAsync(notification));
    await Task.WhenAll(tasks);
    return Results.Ok();
});

app.Run();