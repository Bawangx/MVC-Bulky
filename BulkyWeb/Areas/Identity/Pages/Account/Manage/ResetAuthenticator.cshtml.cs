// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BulkyWeb.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// Halaman ini mengelola proses reset kunci autentikator (authenticator key) untuk fitur 2FA.
    /// Reset ini akan menonaktifkan 2FA sementara sampai pengguna mengonfigurasi ulang aplikasi autentikator mereka.
    /// </summary>
    public class ResetAuthenticatorModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ResetAuthenticatorModel> _logger;

        public ResetAuthenticatorModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ResetAuthenticatorModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Menyimpan pesan status yang akan ditampilkan ke pengguna (misal: sukses, error)
        /// Menggunakan TempData agar pesan dapat bertahan setelah redirect.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Handler untuk HTTP GET. Memastikan user ada, lalu menampilkan halaman reset.
        /// </summary>
        public async Task<IActionResult> OnGet()
        {
            // Mendapatkan user saat ini berdasarkan sesi login
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Jika user tidak ditemukan, tampilkan halaman NotFound dengan pesan error
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Jika user ditemukan, tampilkan halaman reset (razor page ini)
            return Page();
        }

        /// <summary>
        /// Handler untuk HTTP POST saat user menekan tombol reset kunci autentikator.
        /// Proses:
        /// 1. Nonaktifkan 2FA.
        /// 2. Reset kunci autentikator.
        /// 3. Log aktivitas reset.
        /// 4. Refresh sesi login.
        /// 5. Redirect ke halaman konfigurasi ulang autentikator.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // Ambil user saat ini
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Jika user tidak ditemukan, tampilkan NotFound
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Matikan 2FA sementara untuk user ini
            await _userManager.SetTwoFactorEnabledAsync(user, false);

            // Reset kunci autentikator ke nilai baru
            await _userManager.ResetAuthenticatorKeyAsync(user);

            // Log aktivitas reset untuk audit
            _logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Id);

            // Refresh sesi login agar update diterapkan segera
            await _signInManager.RefreshSignInAsync(user);

            // Set pesan status untuk memberi tahu user
            StatusMessage = "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";

            // Redirect ke halaman konfigurasi ulang authenticator app
            return RedirectToPage("./EnableAuthenticator");
        }
    }
}