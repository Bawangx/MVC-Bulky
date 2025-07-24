// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWeb.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// Model untuk halaman manajemen login eksternal.
    /// </summary>
    public class ExternalLoginsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserStore<ApplicationUser> _userStore;

        public ExternalLoginsModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUserStore<ApplicationUser> userStore)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userStore = userStore;
        }

        // Daftar login eksternal yang sudah ditautkan ke akun pengguna.
        public IList<UserLoginInfo> CurrentLogins { get; set; }

        // Daftar provider login eksternal yang tersedia namun belum ditautkan.
        public IList<AuthenticationScheme> OtherLogins { get; set; }

        // Menentukan apakah tombol "Remove" ditampilkan.
        public bool ShowRemoveButton { get; set; }

        // Pesan status untuk ditampilkan di UI.
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Menangani request GET - memuat data login eksternal pengguna.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Memuat login eksternal yang sudah ada.
            CurrentLogins = await _userManager.GetLoginsAsync(user);

            // Memuat provider login eksternal lain yang tersedia, kecuali yang sudah ditautkan.
            OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();

            // Mengecek apakah pengguna memiliki password lokal.
            string passwordHash = null;
            if (_userStore is IUserPasswordStore<ApplicationUser> userPasswordStore)
            {
                passwordHash = await userPasswordStore.GetPasswordHashAsync(user, HttpContext.RequestAborted);
            }

            // Tampilkan tombol hapus jika pengguna punya password atau lebih dari 1 login eksternal.
            ShowRemoveButton = passwordHash != null || CurrentLogins.Count > 1;

            return Page();
        }

        /// <summary>
        /// Menangani request POST untuk menghapus login eksternal tertentu.
        /// </summary>
        public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
            if (!result.Succeeded)
            {
                StatusMessage = "The external login was not removed.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "The external login was removed.";
            return RedirectToPage();
        }

        /// <summary>
        /// Menangani request POST untuk memulai proses menautkan login eksternal baru.
        /// </summary>
        public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
        {
            // Logout external cookie sebelumnya (jika ada).
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Konfigurasi redirect setelah login eksternal selesai.
            var redirectUrl = Url.Page("./ExternalLogins", pageHandler: "LinkLoginCallback");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));

            // Redirect ke halaman login provider eksternal.
            return new ChallengeResult(provider, properties);
        }

        /// <summary>
        /// Callback yang menangani ketika login eksternal selesai untuk proses penautan.
        /// </summary>
        public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userId = await _userManager.GetUserIdAsync(user);

            // Mengambil informasi login eksternal dari cookie.
            var info = await _signInManager.GetExternalLoginInfoAsync(userId);
            if (info == null)
            {
                throw new InvalidOperationException($"Unexpected error occurred loading external login info.");
            }

            // Menautkan login eksternal ke akun pengguna.
            var result = await _userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                StatusMessage = "The external login was not added. External logins can only be associated with one account.";
                return RedirectToPage();
            }

            // Logout external cookie setelah selesai.
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            StatusMessage = "The external login was added.";
            return RedirectToPage();
        }
    }
}