using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SportsArenaWebApi_Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<SportsArenaDbContext>(op => 
op.UseSqlServer(builder.Configuration.GetConnectionString("projectConString")));

// register services
//builder.Services.AddScoped<TbluserService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Otp sending and Storing functionality
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(op =>
{
    op.IOTimeout = TimeSpan.FromMinutes(5);
    op.Cookie.HttpOnly = true;
    op.Cookie.IsEssential = true;
    op.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Important for security
    op.Cookie.SameSite = SameSiteMode.None;
});

// Retrieve JWT key and decode if Base64-encoded
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]); // Load JWT Key

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(op =>
    {
        op.RequireHttpsMetadata = false;
        op.SaveToken = true;
        op.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key), // Ensure key is set properly
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // Ensure token expiration is validated
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // Matches appsettings.json
            ValidAudience = builder.Configuration["Jwt:Audience"], // Matches appsettings.json
        };
    });

builder.Services.AddAuthorization();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod());
});


var app = builder.Build();

app.UseCors("AllowAll");

app.UseSession();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

