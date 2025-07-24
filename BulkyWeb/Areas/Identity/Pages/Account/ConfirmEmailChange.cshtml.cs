// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    public class ConfirmEmailChangeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Konstruktor menerima UserManager dan SignInManager melalui dependency injection
        public ConfirmEmailChangeModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Pesan status yang akan ditampilkan di UI (disimpan di TempData agar tetap ada setelah redirect)
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Handler untuk permintaan GET ke halaman ini.
        /// Parameter: userId, email baru, dan kode token yang diperlukan untuk validasi.
        /// </summary>
        public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
        {
            // Jika parameter tidak lengkap, redirect ke halaman utama
            if (userId == null || email == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            // Cari user berdasarkan ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Jika user tidak ditemukan, kembalikan error 404 dengan pesan yang jelas
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            // Decode kode token dari Base64 URL encoding menjadi string asli
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            // Proses penggantian email menggunakan token yang sudah didecode
            var result = await _userManager.ChangeEmailAsync(user, email, code);
            if (!result.Succeeded)
            {
                // Jika gagal, set pesan error dan tetap di halaman ini
                StatusMessage = "Error changing email.";
                return Page();
            }

            // Karena di UI, email juga digunakan sebagai username,
            // maka username juga harus diupdate agar sesuai dengan email baru
            var setUserNameResult = await _userManager.SetUserNameAsync(user, email);
            if (!setUserNameResult.Succeeded)
            {
                // Jika update username gagal, set pesan error dan tetap di halaman ini
                StatusMessage = "Error changing user name.";
                return Page();
            }

            // Refresh session sign-in agar informasi user terbaru langsung dipakai
            await _signInManager.RefreshSignInAsync(user);

            // Set pesan sukses konfirmasi perubahan email
            StatusMessage = "Thank you for confirming your email change.";

            // Render halaman ini dengan pesan sukses
            return Page();
        }
    }
}