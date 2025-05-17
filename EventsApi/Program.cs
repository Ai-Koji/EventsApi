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

app.MapGet("/events", () =>
{
    return events;
});
app.MapGet("/events/{id}", (int id) =>
{
    var item = events.Find(o => o.Id == id);
    return item;
});
app.MapPost("/events", (EventItem item) =>
{
    item.Id = nextId++;
    events.Add(item);
    return 200;
});
app.MapPut("/events/{id}", (int id, EventItem item) =>
{
    events[id] = item;
    return 200;
});
app.MapDelete("/events{id}", (int id) =>
{
    var element = events.Find(o => o.Id == id);

    if (element != null)
    {
        events.Remove(element);
        return 200;
    }
    return 404;
});


app.Run();

[JsonSerializable(typeof(List<EventItem>))]
[JsonSerializable(typeof(EventItem))]
[JsonSerializable(typeof(ProblemDetails))] // �������� ��� ������
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}