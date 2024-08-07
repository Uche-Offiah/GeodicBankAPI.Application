using AspNetCoreRateLimit;
using GeodicBankAPI.Application.Interfaces;
using GeodicBankAPI.Application.Services;
using GeodicBankAPI.Domain;
using GeodicBankAPI.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SignalRChatApplication;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
    };
});
builder.Services.AddAuthorization();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();
builder.Services.AddTransient<IGeodicTransaction, TransactionService>(); // Modified to avoid ambiguous ref with Redis
builder.Services.AddTransient<IUser, UserService>();

builder.Services.AddDbContext<FinancialDbContext>(options =>
options.UseSqlServer(builder.Configuration["ConnectionStrings:FinancialDbContext"]));

builder.Services.AddScoped<User>();
builder.Services.AddScoped<Transaction>();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
builder.WithOrigins(
    "http://localhost:7156",
    "https://localhost:5131").AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

// Add Rate Limiting
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
//builder.Services.AddSingleton<IRateLimitStore<RateLimitRule>, MemoryCacheRateLimitStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
         new RateLimitRule
         {
             Endpoint = "*",
             Period = "1m",
             Limit = 100
         }
    };
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

//Configure Redis for distributed caching and message persistence
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration["RedisConnection"]);
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseAuthorization();

IConfiguration configuration = app.Configuration;

IWebHostEnvironment environment = app.Environment;

app.MapControllers();
app.MapHub<ChatHub>("/chat");

using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;

    var context = services.GetRequiredService<FinancialDbContext>();
    context.Database.Migrate();
}

//var context = app.Services.GetRequiredService<>();


app.Run();
