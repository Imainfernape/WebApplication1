using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApplication1.Model;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuthDbContext _dbContext;

        public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, AuthDbContext dbContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            LModel.Email = SanitizeInput(LModel.Email);

            var user = await _userManager.FindByEmailAsync(LModel.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return Page();
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError("", "Your account is locked. Try again later.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(LModel.Email, LModel.Password, LModel.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(user);

                string sessionToken = GenerateSecureToken();

                await TrackSessionAsync(user.Id, sessionToken);

                HttpContext.Session.SetString("SessionToken", sessionToken);

                return RedirectToPage("Index");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Your account has been locked due to multiple failed login attempts. Try again later.");
                return Page();
            }
            else
            {
                int failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
                int attemptsLeft = 3 - failedAttempts;

                ModelState.AddModelError("", $"Invalid login attempt. {attemptsLeft} attempts left before lockout.");
                return Page();
            }
        }

        private string SanitizeInput(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        private async Task TrackSessionAsync(string userId, string sessionToken)
        {
            var existingSession = await _dbContext.UserSessions.FirstOrDefaultAsync(s => s.UserId == userId);
            if (existingSession != null)
            {
                existingSession.SessionToken = sessionToken;
                existingSession.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                _dbContext.UserSessions.Add(new UserSession
                {
                    UserId = userId,
                    SessionToken = sessionToken,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync();
        }

        private string GenerateSecureToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}