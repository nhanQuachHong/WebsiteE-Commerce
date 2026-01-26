using System.ComponentModel.DataAnnotations;

namespace WebsiteE_Commerce.Models.ViewModels.Manage
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        [Display(Name = "Nhập lại mật khẩu mới")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
