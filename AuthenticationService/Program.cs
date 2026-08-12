using System.Security.Claims;
using System.Text;
using AuthenticationService;
using AuthenticationService.GrpcServices;
using AuthenticationService.Interfaces;
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

var userServiceGrpcAddress = builder.Configuration["Grpc:UserService"]
    ?? throw new InvalidOperationException("Grpc:UserService is not configured.");

builder.Services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
{
    o.Address = new Uri(userServiceGrpcAddress);
});
builder.Services.AddScoped<IUserClientService, GrpcUserClientService>();

builder.Services.AddGrpcClient<UserSessionGrpc.UserSessionGrpcClient>(o =>
{
    o.Address = new Uri(userServiceGrpcAddress);
});

builder.Services.AddScoped<GrpcUserSessionClientService>();


var configuration = builder.Configuration;

var app = builder.Build();

app.UseCors("AllowAll");

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
