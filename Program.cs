using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineParkingLotSystem.Application.Services;
using OnlineParkingLotSystem.Domain.Strategies;
using OnlineParkingLotSystem.Infrastructure.Data;
using OnlineParkingLotSystem.Middleware;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Swagger
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

//test code for swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});



builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
    options.UseMySql(connectionString, serverVersion);
});

builder.Services.AddScoped<IParkingService, ParkingService>();//scoped service for parking service (once per request)
builder.Services.AddScoped<IAuthService, AuthService>();//scoped service for authentication service
//WHY SCOPED??? services using DbContext should almost always be scoped to avoid data inconsistencies

builder.Services.AddSingleton<IFeeStrategy, HourlyFeeStrategy>();//signgleton cuz it only calc fee so every request can use the same instance
builder.Services.AddSingleton<IFeeStrategy, DailyFeeStrategy>();//they dont remember any state so they can be singleton
builder.Services.AddSingleton<IFeeStrategy, FlatFeeStrategy>();
builder.Services.AddSingleton<FeeStrategyResolver>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {//to validate the token ke hum isko accept karen ya nahin
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,//who created this token
            ValidateAudience = true,//who is this token for
            ValidateLifetime = true,//is this token expired or not
            ValidateIssuerSigningKey = true,//is this token signed with the correct key
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await DbSeeder.SeedAsync(context);
}

app.Run();
