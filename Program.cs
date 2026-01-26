using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebsiteE_Commerce.Data;
using WebsiteE_Commerce.Models;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext cho Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Nếu dùng SQL Server:
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // Nếu dùng provider khác (SQLite/MySQL...) thì thay UseSqlServer bằng UseSqlite / UseMySql...
});

// Identity + Roles
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password policy (có thể nới lỏng cho đồ án, nhưng khuyến nghị giữ mức tối thiểu)
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

        // Lockout khi đăng nhập sai nhiều lần (chống brute-force)
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

        // Unique email
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cookie config (Identity mặc định dùng cookie)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.Cookie.Name = "WebsiteECommerce.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Dev: SameAsRequest; Production: Always

    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Quan trọng: Authentication phải đứng trước Authorization
app.UseAuthentication();
app.UseAuthorization();

// Seed roles + admin mặc định
await IdentitySeedData.SeedAsync(app);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
