using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Configurations;
using TaskService.Interfaces;
using TaskService;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Common.Auth;
using NotificationService.Protos;
using TaskService.GrpcServices;
using UserService.Protos;


var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
var userServiceGrpcAddress = builder.Configuration["Grpc:UserService"]
    ?? throw new InvalidOperationException("Grpc:UserService is not configured.");
var notificationServiceGrpcAddress = builder.Configuration["Grpc:NotificationService"]
    ?? throw new InvalidOperationException("Grpc:NotificationService is not configured.");

builder.Services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
{
    o.Address = new Uri(userServiceGrpcAddress);
});
builder.Services.AddScoped<GrpcUserClientService>();

builder.Services.AddGrpcClient<NotificationGrpc.NotificationGrpcClient>(o =>
{
    o.Address = new Uri(notificationServiceGrpcAddress);
});
builder.Services.AddScoped<GrpcNotificationClientService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTaskServices();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
