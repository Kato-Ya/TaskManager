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
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost")); // Redis!!!!!  redis:6379
builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();

var app = builder.Build();

app.MapGrpcService<GrpcNotificationServerService>();
app.MapControllers();
app.MapGet("/", () => "~NotificationService is running~");

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(8080);
//    options.ListenAnyIP(8081, listenOptions =>
//    {
//        listenOptions.Protocols = HttpProtocols.Http2;
//    });
//});


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
