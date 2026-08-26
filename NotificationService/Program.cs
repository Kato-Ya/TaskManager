using Common.Auth;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using NotificationService;
using NotificationService.GrpcServices;
using NotificationService.Interfaces;
using NotificationService.Services;
using StackExchange.Redis;
using UserService.Protos;

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
builder.Services.AddNotificationServices();

builder.Services.AddGrpc(); 
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("ConnectionStrings:Redis is not configured.");
var userServiceGrpcAddress = builder.Configuration["Grpc:UserService"]
    ?? throw new InvalidOperationException("Grpc:UserService is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
{
    o.Address = new Uri(userServiceGrpcAddress);
});
builder.Services.AddScoped<GrpcUserClientService>();

builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();


var app = builder.Build();

app.UseCors("AllowAll");

app.MapGrpcService<GrpcNotificationServerService>();
app.MapGet("/NotificationService", () => "~NotificationService is running~");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
