using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApplication1.Helper;
using WebApplication1.Model;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly AuthDbContext _dbContext;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, AuthDbContext dbContext, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public void OnGet()
        {
            ViewData["RecaptchaSiteKey"] = _configuration["Recaptcha:SiteKey"];
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var recaptchaResponse = Request.Form["g-recaptcha-response"];
            if (!await ValidateRecaptchaAsync(recaptchaResponse))
            {
                ModelState.AddModelError("", "reCAPTCHA validation failed.");
                return Page();
            }

            RModel.FirstName = SanitizeInput(RModel.FirstName);
            RModel.LastName = SanitizeInput(RModel.LastName);
            RModel.Email = SanitizeInput(RModel.Email);
            RModel.WhoAmI = SanitizeInput(RModel.WhoAmI);

            if (RModel.Resume != null)
            {
                var allowedExtensions = new[] { ".docx", ".pdf" };
                var fileExtension = Path.GetExtension(RModel.Resume.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("RModel.Resume", "Resume must be a .docx or .pdf file.");
                    return Page();
                }

                if (RModel.Resume.Length > 2 * 1024 * 1024) 
                {
                    ModelState.AddModelError("RModel.Resume", "Resume file size must be below 2MB.");
                    return Page();
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, Path.GetFileName(RModel.Resume.FileName));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await RModel.Resume.CopyToAsync(stream);
                }
            }

            var existingUser = await _userManager.FindByEmailAsync(RModel.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("RModel.Email", $"Email '{RModel.Email}' is already taken.");
                return Page();
            }

            if (IsCommonPassword(RModel.Password))
            {
                ModelState.AddModelError("RModel.Password", "Password is too common. Choose a stronger one.");
                return Page();
            }

            var (encryptedNRIC, encryptedAESKey) = EncryptionHelper.EncryptWithKey(RModel.NRIC);

            var user = new IdentityUser
            {
                UserName = RModel.Email,
                Email = RModel.Email
            };

            var result = await _userManager.CreateAsync(user, RModel.Password);

            if (result.Succeeded)
            {
                _dbContext.UserProfiles.Add(new UserProfile
                {
                    UserId = user.Id,
                    FirstName = RModel.FirstName,
                    LastName = RModel.LastName,
                    Gender = RModel.Gender,
                    DateOfBirth = RModel.DateOfBirth,
                    WhoAmI = RModel.WhoAmI
                });

                _dbContext.CustomerData.Add(new CustomerData
                {
                    UserId = user.Id,
                    EncryptedNRIC = encryptedNRIC,
                    EncryptedAESKey = encryptedAESKey
                });

                await _dbContext.SaveChangesAsync();
                await _signInManager.SignInAsync(user, false);
                return RedirectToPage("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        private async Task<bool> ValidateRecaptchaAsync(string recaptchaResponse)
        {
            using var client = new HttpClient();
            var secretKey = _configuration["Recaptcha:SecretKey"];
            var response = await client.GetStringAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={recaptchaResponse}");

            var json = JsonDocument.Parse(response);
            return json.RootElement.GetProperty("success").GetBoolean();
        }

        private bool IsCommonPassword(string password)
        {
            string[] commonPasswords = { "password123", "12345678", "qwerty", "abc123", "iloveyou", "1234567890" };
            return commonPasswords.Contains(password.ToLower());
        }

        private string SanitizeInput(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}
