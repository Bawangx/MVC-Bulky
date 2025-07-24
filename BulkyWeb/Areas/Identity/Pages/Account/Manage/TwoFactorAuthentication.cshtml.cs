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
    /// PageModel untuk halaman Two-factor Authentication (2FA)
    /// Mengelola status 2FA user, recovery codes, dan browser yang diingat (remembered browser)
    /// </summary>
    public class TwoFactorAuthenticationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;       // Manajemen user
        private readonly SignInManager<ApplicationUser> _signInManager;   // Manajemen signin & 2FA
        private readonly ILogger<TwoFactorAuthenticationModel> _logger;   // Logging aktivitas

        public TwoFactorAuthenticationModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TwoFactorAuthenticationModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Apakah user sudah men-setup authenticator app (misal Google Authenticator)
        /// </summary>
        public bool HasAuthenticator { get; set; }

        /// <summary>
        /// Jumlah recovery codes yang tersisa (kode darurat jika 2FA utama hilang)
        /// </summary>
        public int RecoveryCodesLeft { get; set; }

        /// <summary>
        /// Apakah 2FA diaktifkan untuk user ini
        /// </summary>
        [BindProperty]
        public bool Is2faEnabled { get; set; }

        /// <summary>
        /// Apakah browser ini sudah diingat agar tidak perlu memasukkan kode 2FA lagi
        /// </summary>
        public bool IsMachineRemembered { get; set; }

        /// <summary>
        /// Menyimpan pesan status (success/error) yang ditampilkan ke user
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Method dipanggil saat halaman diakses via GET.
        /// Mengambil data 2FA user dari database untuk ditampilkan di UI.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // Ambil user saat ini
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Jika user tidak ditemukan, tampilkan error 404
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Cek apakah user sudah memiliki key authenticator
            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;

            // Cek apakah 2FA diaktifkan user
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            // Cek apakah browser ini sudah diingat oleh sistem (tidak perlu 2FA lagi)
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);

            // Hitung recovery codes yang tersisa untuk user
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

            // Tampilkan halaman dengan data di atas
            return Page();
        }

        /// <summary>
        /// Method dipanggil saat tombol "Forget this browser" ditekan.
        /// Browser akan dihapus dari daftar yang diingat, sehingga saat login berikutnya akan diminta kode 2FA lagi.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // Ambil user saat ini
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Hapus penyimpanan "remember this browser"
            await _signInManager.ForgetTwoFactorClientAsync();

            // Tampilkan pesan konfirmasi ke user
            StatusMessage = "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";

            // Refresh halaman supaya status baru tampil
            return RedirectToPage();
        }
    }
}