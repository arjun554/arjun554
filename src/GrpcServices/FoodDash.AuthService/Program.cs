using FoodDash.AuthService.Services;
using FoodDash.Database;
using FoodDash.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Consul;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddGrpc();

// Add Entity Framework
builder.Services.AddDbContext<FoodDashDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<FoodDashDbContext>()
.AddDefaultTokenProviders();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add Consul for service discovery
builder.Services.AddSingleton<IConsulClient>(p => new ConsulClient(consulConfig =>
{
    consulConfig.Address = new Uri(builder.Configuration.GetConnectionString("Consul") ?? "http://localhost:8500");
}));

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapGrpcService<AuthGrpcService>();

// Register with Consul
var consulClient = app.Services.GetRequiredService<IConsulClient>();
var registration = new AgentServiceRegistration()
{
    ID = $"auth-service-{Environment.MachineName}",
    Name = "auth-service",
    Address = "localhost",
    Port = 5001,
    Tags = new[] { "grpc", "auth" },
    Check = new AgentServiceCheck()
    {
        HTTP = "http://localhost:5001/health",
        Timeout = TimeSpan.FromSeconds(10),
        Interval = TimeSpan.FromSeconds(30)
    }
};

await consulClient.Agent.ServiceRegister(registration);

// Graceful shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(async () =>
{
    await consulClient.Agent.ServiceDeregister(registration.ID);
});

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();