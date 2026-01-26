using Microsoft.AspNetCore.Identity;

namespace WebsiteE_Commerce.Models // Đổi namespace cho đúng với dự án của bạn
{
    public class ApplicationUser : IdentityUser
    {
        public string HoVaTen { get; set; }
        public string DiaChi { get; set; }
    }
}