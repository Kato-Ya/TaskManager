using Authentication.Protos;
using AuthenticationService;
using AuthenticationService.GrpcServices;
using AuthenticationService.Protos;
using static Authentication.Protos.UserSessionGrpc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
