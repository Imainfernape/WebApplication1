using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WebApplication1.Helper;
using WebApplication1.Model; // Import database model

namespace WebApplication1.Pages
{
    [Authorize] // Ensure the user is logged in
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AuthDbContext _dbContext;

        public string DecryptedNRIC { get; private set; }
        public string EncryptedNRIC { get; private set; }

        public IndexModel(
            ILogger<IndexModel> logger,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            AuthDbContext dbContext)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("Login");
            }

            // Retrieve the session token stored in cookies
            var sessionToken = HttpContext.Session.GetString("SessionToken");

            // Validate if the session exists in the database
            var activeSession = await _dbContext.UserSessions
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.SessionToken == sessionToken);

            if (activeSession == null)
            {
                await _signInManager.SignOutAsync();
                HttpContext.Session.Clear();
                return RedirectToPage("Login");
            }

            var customerData = await _dbContext.CustomerData
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customerData != null)
            {
                EncryptedNRIC = customerData.EncryptedNRIC ?? "No Encrypted NRIC Found";

                try
                {
                    DecryptedNRIC = EncryptionHelper.Decrypt(customerData.EncryptedNRIC);
                }
                catch (Exception ex)
                {
                    DecryptedNRIC = $"Error decrypting NRIC: {ex.Message}";
                }
            }
            else
            {
                EncryptedNRIC = "No NRIC data found.";
                DecryptedNRIC = "No NRIC data found.";
            }

            return Page();
        }
    }
}
