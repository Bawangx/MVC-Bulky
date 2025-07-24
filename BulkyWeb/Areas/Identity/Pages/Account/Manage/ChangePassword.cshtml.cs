// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BulkyWeb.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        // Dependency Injection untuk UserManager, SignInManager, dan Logger
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        // Konstruktor untuk inject dependency
        public ChangePasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // Property untuk binding data input dari form (OldPassword, NewPassword, ConfirmPassword)
        [BindProperty]
        public InputModel Input { get; set; }

        // Property untuk menyimpan pesan status yang bisa ditampilkan di UI (misal sukses atau error)
        [TempData]
        public string StatusMessage { get; set; }

        // Kelas yang mendefinisikan input dari user untuk mengubah password
        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        // Method yang dipanggil saat halaman diakses dengan GET
        public async Task<IActionResult> OnGetAsync()
        {
            // Ambil user yang sedang login
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Jika user tidak ditemukan, tampilkan error 404
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Cek apakah user sudah punya password, jika tidak redirect ke halaman set password
            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            // Jika sudah ada password, tampilkan halaman change password
            return Page();
        }

        // Method yang dipanggil saat form dikirim dengan POST
        public async Task<IActionResult> OnPostAsync()
        {
            // Validasi input, jika tidak valid, tampilkan kembali halaman dengan error
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Ambil user yang sedang login
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Coba ganti password dengan input yang diberikan
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                // Jika gagal, tambahkan error ke ModelState agar bisa ditampilkan ke user
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Jika berhasil, refresh sign-in agar security stamp diperbarui
            await _signInManager.RefreshSignInAsync(user);

            // Log aktivitas sukses ganti password
            _logger.LogInformation("User changed their password successfully.");

            // Set pesan status sukses yang akan ditampilkan ke UI
            StatusMessage = "Your password has been changed.";

            // Redirect kembali ke halaman ini agar user lihat pesan sukses
            return RedirectToPage();
        }
    }
}