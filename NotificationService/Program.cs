using Common.Auth;
using NotificationService;
using StackExchange.Redis;
using Common.Messaging;

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

builder.Services.AddRabbitMq(builder.Configuration);
builder.Services.AddHostedService<RabbitMqConsumerWorker>();
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("ConnectionStrings:Redis is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
    redisOptions.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(redisOptions);
});

var app = builder.Build();

app.UseCors("AllowAll");

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
