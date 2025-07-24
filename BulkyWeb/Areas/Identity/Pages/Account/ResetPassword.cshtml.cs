// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Bulky.Models;  // Model user kustom
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;  // Identity framework ASP.NET Core
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities; // Untuk decode kode reset password

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Halaman Razor Page untuk Reset Password
    /// </summary>
    public class ResetPasswordModel : PageModel
    {
        // UserManager digunakan untuk operasi terkait user (password, email, dll)
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Model yang di-bind dari form reset password
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Model input untuk form reset password
        /// </summary>
        public class InputModel
        {
            [Required] // Field wajib diisi
            [EmailAddress] // Validasi format email
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)] // Memberitahu view bahwa ini adalah password
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Code { get; set; }  // Token reset password yang dikirim lewat email
        }

        /// <summary>
        /// Dipanggil saat membuka halaman GET
        /// </summary>
        /// <param name="code">Token reset password, harus di-passing sebagai query string</param>
        /// <returns>Halaman reset password atau error kalau kode tidak ada</returns>
        public IActionResult OnGet(string code = null)
        {
            if (code == null)
            {
                // Kode wajib ada, kalau tidak tampilkan error 400 BadRequest
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                // Decode token dari base64 url ke string biasa agar bisa dipakai
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };
                return Page();
            }
        }

        /// <summary>
        /// Dipanggil saat form reset password di-submit (POST)
        /// </summary>
        /// <returns>Redirect ke halaman konfirmasi kalau berhasil, atau tampilkan form lagi kalau gagal</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            // Validasi model (email, password, konfirmasi, kode)
            if (!ModelState.IsValid)
            {
                // Jika ada error validasi, tampilkan halaman form lagi beserta pesan error
                return Page();
            }

            // Cari user berdasarkan email yang diinput
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Jika user tidak ditemukan, jangan beri tahu pengguna untuk alasan keamanan
                // Langsung redirect ke halaman konfirmasi reset password berhasil
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            // Lakukan reset password menggunakan token (kode) dan password baru
            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                // Jika berhasil, redirect ke halaman konfirmasi sukses
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            // Jika gagal, tambahkan error ke ModelState agar bisa ditampilkan di halaman
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            // Tampilkan halaman form lagi dengan pesan error
            return Page();
        }
    }
}
