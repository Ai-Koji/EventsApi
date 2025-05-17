using EventsApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend"
    , policy =>
    {
        policy
    .WithOrigins("http://localhost:3000"
    ,
    "https://myapp.example.com")
    .AllowAnyMethod() // GET, POST, PUT, DELETE и т.д.
    .AllowAnyHeader() // любой заголовок
    .AllowCredentials(); // если нужно передавать/куки авторизационные заголовки
    });
});

var app = builder.Build();
app.UseCors("AllowFrontend");


var events = new List<EventItem>();
var nextId = 1;

app.Run();