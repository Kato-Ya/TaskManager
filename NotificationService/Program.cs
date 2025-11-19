using Microsoft.AspNetCore.Server.Kestrel.Core;
using Notification.Protos;
using NotificationService;
using NotificationService.GrpcServices;
using NotificationService.Interfaces;
using NotificationService.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddNotificationServices();

builder.Services.AddGrpc(); 
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("Redis")); // Redis!!!!! redis:6379 localhost

builder.Services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
{
    o.Address = new Uri("http://userservice:8080");
});
builder.Services.AddScoped<GrpcUserClientService>();

builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();


var app = builder.Build();

app.MapGrpcService<GrpcNotificationServerService>();
app.MapControllers();
app.MapGet("/NotificationService", () => "~NotificationService is running~");

app.MapGet("/user", async (GrpcUserClientService clientUserService) =>
{
    var user = await clientUserService.GetUserByIdAsync(1);
    return user is not null ? $"User: {user.Username} with id: {user.Id} " : "User not found";
});

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
