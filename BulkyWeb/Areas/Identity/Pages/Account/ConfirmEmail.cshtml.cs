// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Text;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Halaman untuk mengonfirmasi email pengguna ketika mereka mengklik tautan konfirmasi.
    /// </summary>
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        // Konstruktor menerima UserManager yang digunakan untuk mengelola user.
        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Pesan status yang akan ditampilkan ke pengguna setelah proses konfirmasi email.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Method ini dipanggil saat halaman diakses dengan metode GET.
        /// Proses konfirmasi email dilakukan di sini berdasarkan userId dan kode token.
        /// </summary>
        /// <param name="userId">ID pengguna yang akan dikonfirmasi emailnya.</param>
        /// <param name="code">Token konfirmasi email yang dikodekan dalam URL.</param>
        /// <returns>Halaman dengan pesan status sukses/gagal konfirmasi email.</returns>
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            // Jika parameter userId atau code tidak ada, redirect ke halaman utama.
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            // Cari user berdasarkan userId.
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Jika user tidak ditemukan, tampilkan error 404 dengan pesan.
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            // Decode kode token yang sebelumnya di-encode untuk URL.
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            // Panggil fungsi konfirmasi email pada user manager.
            var result = await _userManager.ConfirmEmailAsync(user, code);

            // Set pesan status berdasarkan hasil operasi.
            StatusMessage = result.Succeeded
                ? "Thank you for confirming your email."
                : "Error confirming your email.";

            // Tampilkan halaman dengan pesan status.
            return Page();
        }
    }
}