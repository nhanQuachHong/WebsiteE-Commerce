using System.ComponentModel.DataAnnotations;

namespace WebsiteE_Commerce.Models.ViewModels.Account
{
    public class LoginWith2faViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã 2FA.")]
        [Display(Name = "Mã xác thực (Authenticator Code)")]
        public string TwoFactorCode { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
        public bool RememberMachine { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
