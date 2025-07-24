// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Bulky.Models;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        // Konstruktor menerima dependency injection untuk SignInManager dan Logger
        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Model untuk menerima input dari form login
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// List provider external login (Google, Facebook, dll)
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        /// URL tujuan setelah login berhasil
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Pesan error sementara yang bisa dipakai untuk menampilkan pesan kesalahan antar request
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Kelas internal untuk input form login
        /// </summary>
        public class InputModel
        {
            [Required] // Wajib diisi
            [EmailAddress] // Format harus email valid
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)] // Input tipe password
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; } // Opsi untuk menyimpan login
        }

        // Handler ketika GET request dipanggil (halaman login dimuat)
        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                // Jika ada pesan error, tambahkan ke ModelState agar tampil di halaman
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Jika returnUrl null, set default ke root "/"
            returnUrl ??= Url.Content("~/");

            // Bersihkan cookie external login yang mungkin tersisa sebelumnya
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Ambil daftar external login provider yang tersedia
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        // Handler ketika POST request dipanggil (form login disubmit)
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Jika returnUrl null, set ke root "/"
            returnUrl ??= Url.Content("~/");

            // Ambil daftar external login provider lagi
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid) // Pastikan input valid (required, format email, dll)
            {
                // Lakukan proses login dengan email dan password
                // lockoutOnFailure false artinya kesalahan password tidak akan mengunci akun
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded) // Jika login berhasil
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl); // Redirect ke halaman tujuan
                }
                if (result.RequiresTwoFactor) // Jika butuh autentikasi 2 faktor
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut) // Jika akun terkunci
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else // Jika login gagal tapi bukan terkunci atau 2FA
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // Jika input tidak valid, tampilkan ulang form dengan pesan error
            return Page();
        }
    }
}