using Microsoft.AspNetCore.Server.Kestrel.Core;
using NotificationService.Interfaces;
using NotificationService.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpc(); 
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("Redis")); // Redis!!!!! redis:6379 localhost
builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();

var app = builder.Build();

app.MapGrpcService<GrpcNotificationServerService>();
app.MapControllers();
app.MapGet("/NotificationService", () => "~NotificationService is running~");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
