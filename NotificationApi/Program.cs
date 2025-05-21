using NotificationApi.Services;
using NotificationApi;
using NotificationApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true).AddEnvironmentVariables();
builder.Services.AddSingleton<INotificationSender, ConsoleNotificationSender>();
builder.Services.AddHttpClient<TelegramNotificationSender>();
builder.Services.AddSingleton<INotificationSender>(sp =>

sp.GetRequiredService<TelegramNotificationSender>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Events API", Version = "v1" });
});

var app = builder.Build();

// Настройка Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Events API v1");
    });
}

app.MapPost("/api/notifications"
, async (NotificationDto notification,
IEnumerable<INotificationSender> senders) =>
{
    var tasks = senders.Select(s => s.SendAsync(notification));
    await Task.WhenAll(tasks);
    return Results.Ok();
});

app.Run();