// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Bulky.Models;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginWith2faModel> _logger;

        // Constructor untuk menginisialisasi dependency yang dibutuhkan
        public LoginWith2faModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginWith2faModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // Model yang akan terbind ke form input pada halaman
        [BindProperty]
        public InputModel Input { get; set; }

        // Menyimpan status RememberMe agar tetap login
        public bool RememberMe { get; set; }

        // URL yang akan dituju setelah login sukses
        public string ReturnUrl { get; set; }

        // Model untuk data input 2FA
        public class InputModel
        {
            // Kode autentikator 2FA, minimal 6 maksimal 7 karakter
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string TwoFactorCode { get; set; }

            // Pilihan untuk mengingat mesin (browser) ini agar tidak perlu 2FA saat berikutnya
            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        // Handler GET, menyiapkan halaman saat pertama kali dibuka
        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            // Pastikan user sudah melewati proses username & password
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                // Jika user tidak ditemukan, lempar exception agar tahu ada masalah
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            // Simpan URL tujuan dan rememberMe untuk diteruskan ke halaman
            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }

        // Handler POST, proses submit form 2FA
        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            // Validasi model, jika input tidak valid tampilkan kembali halaman dengan error
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Jika returnUrl kosong, arahkan ke root ("/")
            returnUrl ??= Url.Content("~/");

            // Ambil user yang sedang proses 2FA
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            // Bersihkan input kode 2FA dari spasi dan strip jika ada
            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            // Proses login dengan 2FA
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

            var userId = await _userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                // Jika berhasil, log info dan redirect ke returnUrl
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                // Jika akun terkunci, log warning dan arahkan ke halaman lockout
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                // Jika kode salah, log warning dan tampilkan error di halaman
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return Page();
            }
        }
    }
}