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
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("Redis")); // Redis!!!!! redis:6379 localhost

builder.Services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
{
    o.Address = new Uri("http://userservice:8080");
});
builder.Services.AddScoped<GrpcUserClientService>();

builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();


var app = builder.Build();

app.UseCors("AllowAll");

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
