using Data.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ModelDbContext>();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10); // Th?i gian h?t h?n session
    options.Cookie.HttpOnly = true; // Cookie ch? có th? ???c truy c?p t? máy ch?
    options.Cookie.IsEssential = true; // Cookie c?n thi?t cho ?ng d?ng
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // S? d?ng session


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AuthMiddleware}/{action=Login}");

app.Run();
