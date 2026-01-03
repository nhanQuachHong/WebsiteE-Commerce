using Microsoft.EntityFrameworkCore;
using WebsiteE_Commerce.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký dịch vụ MVC (Controller và View)
builder.Services.AddControllersWithViews();

// 2. Cấu hình kết nối Database (Dựa trên context của Thanh)
builder.Services.AddDbContext<QuanLyHangHoaContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("QuanLyHangHoaContext"));
});

var app = builder.Build();

// 3. Cấu hình luồng xử lý (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Cho phép truy cập ảnh trong wwwroot/images

app.UseRouting();
app.UseAuthorization();

// 4. Cấu hình đường dẫn mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();