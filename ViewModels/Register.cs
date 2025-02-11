using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.ViewModels
{
    public class Register
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First Name must contain only letters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last Name must contain only letters.")]
        public string LastName { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[ST]\d{7}[A-Z]$", ErrorMessage = "Invalid NRIC format. Must start with 'S' or 'T', followed by 7 digits, and end with a letter.")]
        public string NRIC { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
            ErrorMessage = "Password must contain at least one uppercase, one lowercase, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(Register), nameof(ValidateDateOfBirth))]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public IFormFile Resume { get; set; }

        [MaxLength(500, ErrorMessage = "Who Am I section cannot exceed 500 characters.")]
        public string WhoAmI { get; set; } = string.Empty;

        public static ValidationResult? ValidateDateOfBirth(DateTime dateOfBirth, ValidationContext context)
        {
            if (dateOfBirth > DateTime.Today)
                return new ValidationResult("Date of Birth cannot be in the future.");
            return ValidationResult.Success;
        }
    }
}
