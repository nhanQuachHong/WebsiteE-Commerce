using System.ComponentModel.DataAnnotations;

namespace WebsiteE_Commerce.Models.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
