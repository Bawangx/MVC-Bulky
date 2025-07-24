// File ini menangani halaman konfirmasi pendaftaran user baru,
// menampilkan instruksi atau link konfirmasi email setelah registrasi.

#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using Bulky.Models; // Model ApplicationUser
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // UserManager dll
using Microsoft.AspNetCore.Identity.UI.Services; // IEmailSender (untuk kirim email)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities; // Untuk encode URL token

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous] // Halaman ini bisa diakses tanpa login
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _sender;

        // Konstruktor, menerima dependensi UserManager & IEmailSender lewat DI
        public RegisterConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        // Email user yang baru saja registrasi
        public string Email { get; set; }

        // Flag untuk tampilkan link konfirmasi langsung (bila email sender belum aktif)
        public bool DisplayConfirmAccountLink { get; set; }

        // Link konfirmasi email yang akan dikirim atau ditampilkan di halaman
        public string EmailConfirmationUrl { get; set; }

        // Method yang dipanggil ketika halaman diakses via GET request
        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            // Jika parameter email tidak ada, redirect ke homepage
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            // Default returnUrl ke root website jika null
            returnUrl = returnUrl ?? Url.Content("~/");

            // Cari user berdasarkan email yang diberikan
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Jika user tidak ditemukan, tampilkan error 404 dengan pesan
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;

            // Jika belum ada email sender sesungguhnya, tampilkan link konfirmasi langsung
            DisplayConfirmAccountLink = true;

            if (DisplayConfirmAccountLink)
            {
                // Dapatkan UserId
                var userId = await _userManager.GetUserIdAsync(user);

                // Generate token konfirmasi email (berbentuk string)
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Encode token ke Base64 URL safe agar bisa dilewatkan di URL
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                // Buat URL lengkap menuju halaman konfirmasi email, dengan parameter userId & token
                EmailConfirmationUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);
            }

            // Render halaman Razor dengan data yang sudah diisi
            return Page();
        }
    }
}