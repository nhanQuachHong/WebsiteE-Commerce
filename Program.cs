using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebsiteE_Commerce.Data;
using WebsiteE_Commerce.Models;
using WebsiteE_Commerce.Security;
using WebsiteE_Commerce.Services.Email;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// DbContext Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Identity + Roles + Token providers (email confirm / reset password / 2FA)
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Bắt buộc xác thực email trước khi đăng nhập
        options.SignIn.RequireConfirmedEmail = true;

        // Password policy (có thể điều chỉnh theo tiêu chí đồ án)
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

        // Lockout chống brute-force
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

        // Unique email
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cookie config
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.Cookie.Name = "WebsiteECommerce.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Production: Always

    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

// Authorization Policy: Admin bắt buộc bật 2FA mới truy cập admin
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin2FA", policy =>
    {
        policy.RequireRole(AppRoles.Admin);
        policy.AddRequirements(new RequireTwoFactorEnabledRequirement());
    });
});
builder.Services.AddScoped<IAuthorizationHandler, RequireTwoFactorEnabledHandler>();

// Email sender (dev: log console). Bạn có thể thay bằng SMTP sender nếu cần.
builder.Services.AddTransient<IAppEmailSender, ConsoleEmailSender>();

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

// Quan trọng: Authentication trước Authorization
app.UseAuthentication();
app.UseAuthorization();

// Seed roles + admin
await IdentitySeedData.SeedAsync(app);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
