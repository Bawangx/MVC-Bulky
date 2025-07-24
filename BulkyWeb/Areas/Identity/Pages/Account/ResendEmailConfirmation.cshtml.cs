// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    // Mengizinkan akses untuk semua pengguna, termasuk yang belum login
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;  // Manajemen user
        private readonly IEmailSender _emailSender;                    // Layanan untuk mengirim email

        // Konstruktor untuk inject dependency UserManager dan IEmailSender
        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // Property Input yang di-bind ke form
        [BindProperty]
        public InputModel Input { get; set; }

        // Model input yang hanya berisi Email, dengan validasi data annotation
        public class InputModel
        {
            [Required(ErrorMessage = "Email wajib diisi.")]
            [EmailAddress(ErrorMessage = "Format email tidak valid.")]
            public string Email { get; set; }
        }

        // Handle request GET, biasanya hanya menampilkan halaman kosong
        public void OnGet()
        {
        }

        // Handle request POST saat user submit form
        public async Task<IActionResult> OnPostAsync()
        {
            // Cek validasi input form
            if (!ModelState.IsValid)
            {
                return Page();  // Kembalikan halaman dengan error validasi
            }

            // Cari user berdasarkan email yang dimasukkan
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Tidak memberitahu user bahwa email tidak ditemukan untuk keamanan,
                // tetapi tetap beri pesan sukses bahwa email dikirim
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return Page();
            }

            // Ambil userId dan generate token konfirmasi email
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Encode token ke format yang aman untuk URL
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Buat URL callback untuk konfirmasi email
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",     // Halaman tujuan konfirmasi email
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);

            // Kirim email ke user dengan tautan konfirmasi
            await _emailSender.SendEmailAsync(
                Input.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            // Tampilkan pesan sukses bahwa email sudah dikirim
            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return Page();
        }
    }
}