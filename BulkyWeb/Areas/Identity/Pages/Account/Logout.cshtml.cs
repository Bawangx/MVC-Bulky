// Required licenses and namespaces omitted for brevity

using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        // Constructor menerima SignInManager untuk proses logout dan Logger untuk pencatatan log
        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Method yang dipanggil saat form logout disubmit dengan metode POST
        /// </summary>
        /// <param name="returnUrl">URL tujuan setelah logout (opsional)</param>
        /// <returns>Redirect ke halaman yang sesuai</returns>
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // Proses logout user dari ASP.NET Core Identity
            await _signInManager.SignOutAsync();

            // Log info bahwa user telah logout
            _logger.LogInformation("User logged out.");

            if (returnUrl != null)
            {
                // Redirect ke URL yang diberikan, pastikan URL lokal agar aman
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Redirect ke halaman ini sendiri untuk memicu request baru dan memperbarui identity user
                return RedirectToPage();
            }
        }
    }
}