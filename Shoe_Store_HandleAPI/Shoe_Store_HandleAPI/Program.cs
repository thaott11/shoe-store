using Data.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shoe_Store_HandleAPI.Service;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// C?u hình CORS cho phép frontend k?t n?i
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:5210", "https://localhost:7279")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials(); // Cho phép g?i cookie qua các domain khác nhau
        });
});

// Thêm d?ch v? EmailService và DbContext
builder.Services.AddTransient<EmailService>();
builder.Services.AddDbContext<ModelDbContext>();

// C?u hình JSON options
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.JsonSerializerOptions.WriteIndented = true;
});

// Thêm d?ch v? cache và session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None; // Cho phép chia s? cookie gi?a các domain khác nhau
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ch? g?i cookie qua HTTPS
});

// C?u hình Authentication v?i Cookie và JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, option =>
{
    option.Cookie = new CookieBuilder()
    {
        Name = "Shoe_Store_Cookie",
        HttpOnly = true,
        SecurePolicy = CookieSecurePolicy.Always,
        SameSite = SameSiteMode.None
    };
    option.LoginPath = "/api/AuthMiddlewareAPI/unauthorized";
    option.LogoutPath = "/api/AuthMiddlewareAPI/logout";
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Thêm c?u hình cho controllers và JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });

// C?u hình Swagger cho tài li?u API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins"); // S? d?ng chính sách CORS
app.UseSession(); // S? d?ng session
app.UseAuthentication(); // S? d?ng authentication
app.UseAuthorization(); // S? d?ng authorization

app.MapControllers(); // Map các controllers

app.Run();
