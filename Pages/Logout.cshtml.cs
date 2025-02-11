using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AuthDbContext _dbContext;

        public LogoutModel(SignInManager<IdentityUser> signInManager, AuthDbContext dbContext)
        {
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);
            if (user != null)
            {
                var sessionToken = HttpContext.Session.GetString("SessionToken");

                if (!string.IsNullOrEmpty(sessionToken))
                {
                    await _dbContext.RemoveSessionAsync(user.Id, sessionToken);
                }
            }

            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();

            return RedirectToPage("Login");
        }

        public IActionResult OnPostDontLogout()
        {
            return RedirectToPage("Index");
        }

        public void OnGet()
        {
        }
    }
}
