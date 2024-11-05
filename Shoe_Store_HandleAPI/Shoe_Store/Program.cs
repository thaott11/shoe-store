using Data.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Shoe_Store.Controllers;
using Shoe_Store_HandleAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ModelDbContext>();


builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
 builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/AuthMiddleware/Login";
            options.LogoutPath = "/AuthMiddleware/Logout";
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
        });


var app = builder.Build();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AuthMiddleware}/{action=Login}");

app.Run();
