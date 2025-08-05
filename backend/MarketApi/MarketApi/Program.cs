using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using MarketApi.Models;
using MarketApi.Services;
using MarketApi.Providers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<IStorageProvider>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    if (builder.Environment.IsDevelopment())
    {
        return new LocalFileStorageProvider(config);
    }
    else
    {
        return new CloudStorageProvider(config);
    }
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var allowedOrigin = builder.Configuration.GetValue<string>("AllowedCorsOrigin");
if (string.IsNullOrWhiteSpace(allowedOrigin))
{
    throw new InvalidOperationException("AllowedCorsOrigin is not set in configuration.");
}
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(allowedOrigin)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var tokenKey = builder.Configuration.GetValue<string>("TokenKey");
if (string.IsNullOrEmpty(tokenKey) || tokenKey.Length < 32) // consider 64 for an e-commerce site. People get paranoid when money is on the line.
{
    throw new InvalidOperationException("TokenKey is missing or empty in configuration.");
}

var key = Encoding.ASCII.GetBytes(tokenKey); // ACII or UTF8, look into what is best the best practice.
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
        ?? throw new InvalidOperationException("JwtSettings section is not configured properly.");
    if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
    {
        throw new InvalidOperationException("JwtSettings:Issuer is not set in configuration.");
    }
    if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
    {
        throw new InvalidOperationException("JwtSettings:Audience is not set in configuration.");
    }
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,

        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bitch.IO",
        Version = "v1",
        Description = "A marketplace API with Ethereum/MetaMask auth"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MarketApi v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAngularApp");

app.MapControllers();

app.UseStaticFiles();

app.Run();
