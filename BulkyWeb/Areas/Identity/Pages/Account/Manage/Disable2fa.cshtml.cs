// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BulkyWeb.Areas.Identity.Pages.Account.Manage
{
    public class Disable2faModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager; // Manages user-related actions
        private readonly ILogger<Disable2faModel> _logger;          // For logging activities

        public Disable2faModel(
            UserManager<ApplicationUser> userManager,
            ILogger<Disable2faModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // TempData untuk menyimpan pesan status antar request
        [TempData]
        public string StatusMessage { get; set; }

        // Handler GET: Menampilkan halaman menonaktifkan 2FA
        public async Task<IActionResult> OnGet()
        {
            // Ambil user saat ini berdasarkan konteks login
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Jika user tidak ditemukan, tampilkan error 404
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Pastikan 2FA sudah diaktifkan, kalau belum tidak bisa dinonaktifkan
            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                throw new InvalidOperationException("Cannot disable 2FA for user as it's not currently enabled.");
            }

            return Page();
        }

        // Handler POST: Proses menonaktifkan 2FA setelah submit form
        public async Task<IActionResult> OnPostAsync()
        {
            // Ambil user saat ini
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Jika user tidak ditemukan, tampilkan error 404
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Set 2FA ke false untuk menonaktifkan
            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                // Jika gagal menonaktifkan 2FA, lempar exception
                throw new InvalidOperationException("Unexpected error occurred disabling 2FA.");
            }

            // Catat log bahwa user telah menonaktifkan 2FA
            _logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", _userManager.GetUserId(User));

            // Set pesan status untuk tampilkan ke user
            StatusMessage = "2FA has been disabled. You can re-enable 2FA when you set up an authenticator app.";

            // Redirect kembali ke halaman TwoFactorAuthentication
            return RedirectToPage("./TwoFactorAuthentication");
        }
    }
}