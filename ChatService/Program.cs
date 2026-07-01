using ChatService;
using ChatService.Data;
using ChatService.GrpcServices;
using ChatService.Hubs;
using ChatService.Interfaces;
//using ChatService.Protos;
using UserService.Protos;
using ChatService.Services;
using Common.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NotificationService.Protos;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// JWTBearer setting
builder.Services.AddJwtAuth(builder.Configuration);

// CORS setting
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .WithOrigins("http://localhost:3005")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var configuration = builder.Configuration;
var redisConnectionString = configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("ConnectionStrings:Redis is not configured.");
var userServiceGrpcAddress = configuration["Grpc:UserService"]
    ?? throw new InvalidOperationException("Grpc:UserService is not configured.");
var notificationServiceGrpcAddress = configuration["Grpc:NotificationService"]
    ?? throw new InvalidOperationException("Grpc:NotificationService is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnectionString));

// SignalR + Redis backplane
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnectionString);

builder.Services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
{
    o.Address = new Uri(userServiceGrpcAddress);
});

builder.Services.AddGrpcClient<NotificationGrpc.NotificationGrpcClient>(o =>
{
    o.Address = new Uri(notificationServiceGrpcAddress);
});

builder.Services.AddScoped<IChatService, ChatService.Services.ChatService>();
builder.Services.AddScoped<GrpcUserClientService>();
builder.Services.AddScoped<GrpcNotificationClientService>();
builder.Services.AddMessageServices();

var app = builder.Build();

app.UseCors("AllowAll");

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
