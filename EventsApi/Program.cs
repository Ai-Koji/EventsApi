using EventsApi;
using EventsApi.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend"
    , policy =>
    {
        policy
    .WithOrigins("http://localhost:3000"
    ,
    "https://myapp.example.com")
    .AllowAnyMethod() // GET, POST, PUT, DELETE � �.�.
    .AllowAnyHeader() // ����� ���������
    .AllowCredentials(); // ���� ����� ����������/���� ��������������� ���������
    });
});

var notifBase = builder.Configuration["NotificationService:BaseUrl"]!;
builder.Services.AddHttpClient("notification"
, c =>
c.BaseAddress = new Uri(notifBase));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Events API", Version = "v1" });
});

// ��������� ������������ JSON ������������
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});


var app = builder.Build();
app.UseCors("AllowFrontend");
app.UseMiddleware<LoggingMiddleware>();

// ������������� ����� ���������� ������
app.UseExceptionHandler("/error");
// Endpoint ��� ������
app.Map("/error"
, (HttpContext context, ILogger<Program> logger) =>
{
    var innerException = context.Features.Get<IExceptionHandlerFeature>
    ()?.Error;
    logger.LogError(innerException,
    "Unhandled exception occurred");
    return Results.Problem(
    detail: "���������� ������ �������. ��������� ������ �����.", statusCode: 500);
});

// ��������� Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Events API v1");
    });
}

var events = new List<EventItem>();
var nextId = 1;

// ��������� �������� �������
events.AddRange(new[]
{
    new EventItem
    {
        Id = nextId++,
        Title = "����������� �������������",
        EventDate = DateTime.Now.AddDays(10)
    },
    new EventItem
    {
        Id = nextId++,
        Title = "������� �� ASP.NET Core",
        EventDate = DateTime.Now.AddDays(15)
    },
    new EventItem
    {
        Id = nextId++,
        Title = "���� ����� ������� .NET 8",
        EventDate = DateTime.Now.AddDays(20)
    }
});

app.MapGet("/events", (DateTime? from, DateTime? to, string? sort) =>
{
    var filtered = events;
    if (from != null)
        filtered = filtered.Where(o => o.EventDate >= from).ToList();
    if (to != null)
        filtered = filtered.Where(o => o.EventDate <= to).ToList();

    if (!string.IsNullOrWhiteSpace(sort))
    {
        var s = sort.Trim().ToLowerInvariant();
        if (s == "asc")
        {
            filtered.OrderBy(e => e.EventDate);
        }
        else if (s == "desc")
        {
            filtered.OrderByDescending(e => e.EventDate);
        }
        else
        {
            return Results.BadRequest("�������� 'sort' ����� ���� ������ 'asc' 'desc'.");
        }
    }
    return Results.Ok(filtered.ToList());
});

app.MapGet("/events/{id}", (int id) =>
{
    var item = events.Find(o => o.Id == id);
    return item;
});
app.MapPost("/events", async (EventItem item, IHttpClientFactory httpFactory, ILogger<Program> logger) =>
{
    if (item.EventDate < DateTime.UtcNow)
        return Results.BadRequest("���� ������� �� ����� ���� � �������.");

    item.Id = nextId++;
    events.Add(item);

    // �������� ����������� � Notification API
    try
    {
        var client = httpFactory.CreateClient("notification");
        var notification = new NotificationDto
        {
            EventId = item.Id,
            Title = item.Title,
            EventDate = item.EventDate
        };
        
        var response = await client.PostAsJsonAsync("/api/notifications", notification);
        response.EnsureSuccessStatusCode();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "������ ��� �������� ����������� ��� ������� {EventId}", item.Id);
    }

    return Results.Ok(item);
});
app.MapPut("/events/{id}", (int id, EventItem item) =>
{
    var element = events.Where(o => o.Id == id).FirstOrDefault();
    if (element != null)
        element.Title = item.Title;

    return Results.Ok();
});
app.MapDelete("/events{id}", (int id) =>
{
    var element = events.Find(o => o.Id == id);

    if (element != null)
    {
        events.Remove(element);
        return Results.Ok();
    }
    return Results.BadRequest();
});


app.Run();

[JsonSerializable(typeof(List<EventItem>))]
[JsonSerializable(typeof(EventItem))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(HttpValidationProblemDetails))] // ��������� ��� ��������� ������
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
public class NotificationDto
{
    public int EventId { get; set; }
    public string Title { get; set; }
    public DateTime EventDate { get; set; }
    // ������ ����������� ��� model binding
    public NotificationDto() { }
    public NotificationDto(int eventId, string title, DateTime eventDate)
    {
        EventId = eventId;
        Title = title;
        EventDate = eventDate;
    }
}