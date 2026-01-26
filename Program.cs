using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebsiteE_Commerce.Data; // Namespace chứa QuanLyHangHoaContext
using WebsiteE_Commerce.Models; // Namespace chứa ApplicationUser
// using WebsiteE_Commerce.Models; // Mở comment dòng này nếu ApplicationUser nằm trong thư mục Models

var builder = WebApplication.CreateBuilder(args);

// ==========================
// 1. CẤU HÌNH DỊCH VỤ (SERVICES)
// ==========================

// Đăng ký dịch vụ MVC
builder.Services.AddControllersWithViews();

// Cấu hình kết nối Database
// Nó sẽ đọc chuỗi kết nối "QuanLyHangHoaContext" từ file appsettings.json
builder.Services.AddDbContext<QuanLyHangHoaContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("QuanLyHangHoaContext"));
});

// Cấu hình Identity (Đăng ký, Đăng nhập)
// Lưu ý: ApplicationUser là class bạn tự tạo kế thừa IdentityUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<QuanLyHangHoaContext>()
    .AddDefaultTokenProviders();

// Cấu hình Cookie (Tùy chọn: chỉnh đường dẫn khi chưa đăng nhập)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";       // Chuyển hướng khi chưa đăng nhập
    options.LogoutPath = "/Account/Logout";     // Đường dẫn đăng xuất
    options.AccessDeniedPath = "/Account/AccessDenied"; // Chuyển hướng khi không có quyền
});

// (Tùy chọn) Đăng ký Session nếu bạn làm Giỏ hàng
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ==========================
// 2. CẤU HÌNH PIPELINE (MIDDLEWARE)
// ==========================

// Cấu hình môi trường
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Cho phép truy cập wwwroot (css, js, images)

app.UseRouting();

// [QUAN TRỌNG] Thứ tự Authentication (Xác thực) -> Authorization (Phân quyền)
app.UseAuthentication();
app.UseAuthorization();

// Kích hoạt Session (nếu có dùng giỏ hàng)
app.UseSession();

// Cấu hình định tuyến (Routing)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();