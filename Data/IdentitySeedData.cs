using Microsoft.AspNetCore.Identity;
using WebsiteE_Commerce.Models;
using WebsiteE_Commerce.Security;

namespace WebsiteE_Commerce.Data
{
    public static class IdentitySeedData
    {
        public static async Task SeedAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Roles
            await EnsureRoleAsync(roleManager, AppRoles.Admin);
            await EnsureRoleAsync(roleManager, AppRoles.Seller);
            await EnsureRoleAsync(roleManager, AppRoles.User);

            // Admin default
            var adminEmail = config["SeedAdmin:Email"] ?? "admin@ecommerce.local";
            var adminPassword = config["SeedAdmin:Password"] ?? "Admin@12345";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true // admin seed: cho xác thực sẵn để đăng nhập
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                {
                    var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Seed admin thất bại: {msg}");
                }
            }

            if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
            {
                var result = await userManager.AddToRoleAsync(admin, AppRoles.Admin);
                if (!result.Succeeded)
                {
                    var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Gán role Admin thất bại: {msg}");
                }
            }
        }

        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string role)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Tạo role '{role}' thất bại: {msg}");
                }
            }
        }
    }
}
