// Lisensi penggunaan file
// File ini merupakan bagian dari ASP.NET Core Identity bawaan (default UI)

#nullable disable

using System;
using System.Linq;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BulkyWeb.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// Halaman backend untuk proses generate kode pemulihan (recovery codes) 2FA.
    /// Mengharuskan 2FA aktif sebelum kode dapat dihasilkan.
    /// </summary>
    public class GenerateRecoveryCodesModel : PageModel
    {
        // Dependency Injection untuk UserManager dan Logger
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GenerateRecoveryCodesModel> _logger;

        public GenerateRecoveryCodesModel(
            UserManager<ApplicationUser> userManager,
            ILogger<GenerateRecoveryCodesModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Menyimpan recovery codes yang dihasilkan ke TempData
        /// agar tersedia setelah redirect (dikirim ke ShowRecoveryCodes.cshtml)
        /// </summary>
        [TempData]
        public string[] RecoveryCodes { get; set; }

        /// <summary>
        /// Pesan status antar halaman (juga via TempData).
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Handler GET - hanya mengecek apakah user valid dan sudah aktifkan 2FA.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                // Tidak boleh lanjut jika 2FA belum diaktifkan
                throw new InvalidOperationException($"Cannot generate recovery codes for user because they do not have 2FA enabled.");
            }

            return Page();
        }

        /// <summary>
        /// Handler POST - memproses generate kode recovery baru.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);

            if (!isTwoFactorEnabled)
            {
                // Blokir proses jika user belum aktifkan 2FA
                throw new InvalidOperationException($"Cannot generate recovery codes for user as they do not have 2FA enabled.");
            }

            // Generate 10 kode recovery baru
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            // Simpan ke TempData untuk ditampilkan di halaman ShowRecoveryCodes
            RecoveryCodes = recoveryCodes.ToArray();

            // Logging tindakan user (untuk keperluan audit/admin)
            _logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", userId);

            // Set status pesan dan redirect ke halaman untuk menampilkan kode
            StatusMessage = "You have generated new recovery codes.";
            return RedirectToPage("./ShowRecoveryCodes");
        }
    }
}