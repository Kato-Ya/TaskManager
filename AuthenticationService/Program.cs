using System.Security.Claims;
using System.Text;
using AuthenticationService;
using AuthenticationService.GrpcServices;
using System.Text.Json;
using Common.Auth;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using UserService.Protos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// JWTBearer setting
builder.Services.AddJwtAuth(builder.Configuration);

// CORS Setting
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .WithOrigins("http://localhost:3005", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
        );
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();
builder.Services.AddAuthServices();

builder.Services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
{
    o.Address = new Uri("http://userservice:8080");
});
builder.Services.AddScoped<GrpcUserClientService>();

builder.Services.AddGrpcClient<UserSessionGrpc.UserSessionGrpcClient>(o =>
{
    o.Address = new Uri("http://userservice:8080");
});

builder.Services.AddScoped<GrpcUserSessionClientService>();


var configuration = builder.Configuration;

var app = builder.Build();

app.UseCors("AllowAll");

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
