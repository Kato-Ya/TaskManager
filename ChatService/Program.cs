using ChatService;
using ChatService.Data;
using ChatService.GrpcServices;
using ChatService.Hubs;
using ChatService.Interfaces;
using ChatService.Protos;
using ChatService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var configuration = builder.Configuration;

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("Redis")); // Redis!!!!! redis:6379 localhost

// SignalR + Redis backplane
builder.Services.AddSignalR()
    .AddStackExchangeRedis(configuration.GetConnectionString("Redis")); //Redis

builder.Services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
{
    o.Address = new Uri("http://userservice:8080");
});

builder.Services.AddGrpcClient<NotificationGrpc.NotificationGrpcClient>(o =>
{
    o.Address = new Uri("http://notificationservice:8080");
});

builder.Services.AddScoped<IChatService, ChatService.Services.ChatService>();
builder.Services.AddScoped<GrpcUserClientService>();
builder.Services.AddScoped<GrpcNotificationClientService>();
builder.Services.AddMessageServices();
var app = builder.Build();


app.MapGet("/user", async (GrpcUserClientService clientUserService) =>
{
    var user = await clientUserService.GetUserByIdAsync(1);
    return user is not null ? $"User: {user.Username} with id: {user.Id} " : "User not found";
});

//Keep tracking
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    db.Database.EnsureCreated();
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<ChatHub>("/chatHub");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
