using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.Helper;
using WebApplication1.Model;

namespace WebApplication1.Pages
{
    [Authorize] 
    public class PrivacyModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuthDbContext _dbContext;

        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public string? Email { get; private set; }
        public string? Gender { get; private set; }
        public string? DecryptedNRIC { get; private set; }
        public string? EncryptedNRIC { get; private set; }
        public string? DateOfBirth { get; private set; }
        public string? WhoAmI { get; private set; }

        public PrivacyModel(UserManager<IdentityUser> userManager, AuthDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Email = user.Email;

                var customerData = await _dbContext.CustomerData
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (customerData != null)
                {
                    EncryptedNRIC = customerData.EncryptedNRIC ?? "No Encrypted NRIC Found";

                    try
                    {
                        DecryptedNRIC = EncryptionHelper.Decrypt(customerData.EncryptedNRIC);
                    }
                    catch
                    {
                        DecryptedNRIC = "Error decrypting NRIC";
                    }
                }

                var userProfile = await _dbContext.UserProfiles 
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (userProfile != null)
                {
                    FirstName = userProfile.FirstName;
                    LastName = userProfile.LastName;
                    Gender = userProfile.Gender;
                    DateOfBirth = userProfile.DateOfBirth.ToString("yyyy-MM-dd");
                    WhoAmI = userProfile.WhoAmI;
                }
            }
        }
    }
}
