using Microsoft.EntityFrameworkCore;
using FoodDash.Database;
using FoodDash.OrderService.Services;
using FoodDash.OrderService.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddGrpc();

// Add Entity Framework
builder.Services.AddDbContext<FoodDashDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add application services
builder.Services.AddScoped<IOrderBusinessService, OrderBusinessService>();

// Add gRPC clients for other services
builder.Services.AddGrpcClient<FoodDash.Shared.Grpc.PaymentService.PaymentServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["Services:PaymentService"] ?? "https://localhost:7002");
});

builder.Services.AddGrpcClient<FoodDash.Shared.Grpc.NotificationService.NotificationServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["Services:NotificationService"] ?? "https://localhost:7003");
});

// Add Consul service discovery
builder.Services.AddConsul(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapGrpcService<OrderGrpcService>();

app.MapGet("/", () => "Order Service is running. gRPC endpoint available.");

// Register with Consul
app.RegisterWithConsul();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FoodDashDbContext>();
    context.Database.EnsureCreated();
}

app.Run();