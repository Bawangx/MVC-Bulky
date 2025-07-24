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
    public class ForgotPasswordModel : PageModel
    {
        // Dependency untuk mengelola user di Identity
        private readonly UserManager<ApplicationUser> _userManager;
        // Dependency untuk mengirim email
        private readonly IEmailSender _emailSender;

        // Konstruktor menerima dependency melalui dependency injection
        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // Model input yang di-bind ke form
        [BindProperty]
        public InputModel Input { get; set; }

        // Kelas untuk menyimpan data input dari user
        public class InputModel
        {
            [Required] // Wajib diisi
            [EmailAddress] // Harus format email valid
            public string Email { get; set; }
        }

        // Handler POST ketika form dikirim
        public async Task<IActionResult> OnPostAsync()
        {
            // Validasi model input
            if (ModelState.IsValid)
            {
                // Cari user berdasarkan email
                var user = await _userManager.FindByEmailAsync(Input.Email);
                // Jika user tidak ditemukan atau email belum dikonfirmasi
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Jangan beri tahu user kalau email tidak ada atau belum konfirmasi, langsung redirect ke halaman konfirmasi
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Buat token reset password yang aman
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                // Encode token agar bisa dikirim lewat URL
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                // Buat URL callback untuk reset password, nanti user klik link ini di email
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                // Kirim email berisi link reset password
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                // Setelah email terkirim, redirect ke halaman konfirmasi
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            // Jika validasi gagal, tampilkan kembali form dengan error
            return Page();
        }
    }
}