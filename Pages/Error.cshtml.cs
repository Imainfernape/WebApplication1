using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace WebApplication1.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;

        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public int EStatusCode { get; private set; } = 500;
        public string ErrorMessage { get; private set; } = "An unexpected error has occurred.";

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(int? code = null)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            EStatusCode = code ?? 500;

            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error != null)
            {
                _logger.LogError(exceptionHandlerPathFeature.Error, "An error occurred: {RequestId}", RequestId);
            }

            switch (EStatusCode)
            {
                case 404:
                    ErrorMessage = "Oops! The page you are looking for does not exist.";
                    break;
                case 403:
                    ErrorMessage = "Oi! You don't have permission to access this page.";
                    break;
                case 500:
                    ErrorMessage = "A server error has occurred. Please try again later.";
                    break;
                default:
                    ErrorMessage = "Something went wrong. Please try again.";
                    break;
            }
        }
    }
}
