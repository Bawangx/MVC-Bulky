// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    public class LoginWithRecoveryCodeModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginWithRecoveryCodeModel> _logger;

        // Konstruktor untuk inject dependency seperti SignInManager, UserManager, dan Logger
        public LoginWithRecoveryCodeModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginWithRecoveryCodeModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // Binding model untuk form input recovery code
        [BindProperty]
        public InputModel Input { get; set; }

        // Menyimpan URL tujuan setelah login sukses
        public string ReturnUrl { get; set; }

        // Class yang merepresentasikan data input form
        public class InputModel
        {
            [BindProperty]
            [Required(ErrorMessage = "Recovery code wajib diisi.")]
            [DataType(DataType.Text)]
            [Display(Name = "Recovery Code")]
            public string RecoveryCode { get; set; }
        }

        // Method ini dipanggil saat halaman diakses lewat GET
        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            // Pastikan user sudah melewati proses login username & password sebelumnya (2FA user)
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                // Jika user belum ada, lempar error
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            ReturnUrl = returnUrl;
            return Page();
        }

        // Method ini dipanggil saat form submit (POST)
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Jika validasi input gagal, kembalikan ke halaman form
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Ambil user 2FA yang sedang login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("Unable to load two-factor authentication user.");
            }

            // Hilangkan spasi dari recovery code untuk menghindari error input
            var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

            // Coba login menggunakan recovery code
            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            // Ambil UserId untuk logging
            var userId = await _userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User dengan ID '{UserId}' berhasil login menggunakan recovery code.", user.Id);
                // Redirect ke halaman returnUrl atau homepage jika sukses login
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account terkunci.");
                // Redirect ke halaman lockout jika akun terkunci
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Recovery code tidak valid untuk user dengan ID '{UserId}'.", user.Id);
                // Tampilkan pesan error validasi recovery code
                ModelState.AddModelError(string.Empty, "Recovery code tidak valid.");
                return Page();
            }
        }
    }
}