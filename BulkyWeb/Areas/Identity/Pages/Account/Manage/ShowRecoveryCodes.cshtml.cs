// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWeb.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// Page model untuk menampilkan kode recovery 2FA kepada user.
    /// Kode recovery ini digunakan jika user kehilangan akses ke aplikasi authenticator.
    /// </summary>
    public class ShowRecoveryCodesModel : PageModel
    {
        /// <summary>
        /// Array kode recovery yang disimpan sementara menggunakan TempData.
        /// TempData digunakan agar data ini bisa dipertahankan antar request,
        /// tapi tidak tersimpan permanen di session atau database.
        /// </summary>
        [TempData]
        public string[] RecoveryCodes { get; set; }

        /// <summary>
        /// Pesan status (contoh: berhasil generate kode baru) yang juga disimpan di TempData
        /// agar bisa ditampilkan di halaman view.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Handler HTTP GET untuk menampilkan halaman.
        /// Jika kode recovery tidak ada (null atau kosong),
        /// redirect ke halaman TwoFactorAuthentication karena tidak ada kode untuk ditampilkan.
        /// </summary>
        /// <returns>Halaman ShowRecoveryCodes atau redirect ke halaman TwoFactorAuthentication</returns>
        public IActionResult OnGet()
        {
            // Jika tidak ada recovery codes, redirect ke halaman 2FA
            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }

            // Jika ada, tampilkan halaman ini
            return Page();
        }
    }
}