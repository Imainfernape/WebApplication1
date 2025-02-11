using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace WebApplication1.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ResetPasswordModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required]
            public string Email { get; set; }

            [Required]
            public string Token { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
            [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]+$",
                ErrorMessage = "Password must include at least one uppercase, one lowercase, one number, and one special character.")]
            public string NewPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string token, string email)
        {
            Input.Email = email;
            Input.Token = token;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid or expired password reset request.");
                return Page();
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Token, Input.NewPassword);
            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation"); 
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
