using Data.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ModelDbContext>();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "Shoe_Store_Session";
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict; // S? d?ng Strict ?? tr�nh session cookie b? m?t
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ch? cho ph�p cookie qua HTTPS
});



// C?u h�nh CORS cho ph�p k?t n?i v?i backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.WithOrigins("https://localhost:7279") // Ch? ??nh backend URL
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()); // Cho ph�p g?i cookie trong y�u c?u
});

var app = builder.Build();

app.UseCors("AllowAll"); // �p d?ng ch�nh s�ch CORS

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // C?u h�nh HSTS cho production
    app.UseHsts();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseSession(); // S? d?ng session
app.UseAuthorization(); // S? d?ng authorization

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AuthMiddleware}/{action=Login}");

app.Run();
