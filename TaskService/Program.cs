using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Configurations;
using TaskService.Services;
using TaskService.Interfaces;
using TaskService;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using TaskService.Protos;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
{
    o.Address = new Uri("http://userservice:8081");
});

builder.Services.AddGrpcClient<NotificationGrpc.NotificationGrpcClient>(o =>
{
    o.Address = new Uri("http://notificationservice:8081");
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

app.MapGet("/user", async (GrpcUserClientService clientUserService) =>
{
    var user = await clientUserService.GetUserByIdAsync(1);
    return user is not null ? $"User: {user.Username} with id: {user.Id} " : "User not found";
});

app.MapGet("/notification", async (GrpcNotificationClientService clientService) =>
{
    var notification = await clientService.SendNotificationAsync(1, "message", 1);
    return notification;
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
