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
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        public DeletePersonalDataModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // BindProperty membuat InputModel dapat diisi dari form post
        [BindProperty]
        public InputModel Input { get; set; }

        // Model untuk menerima password dari form saat user menghapus akun
        public class InputModel
        {
            [Required] // Password wajib diisi
            [DataType(DataType.Password)] // Tipe data password agar input disembunyikan
            public string Password { get; set; }
        }

        // Properti untuk menentukan apakah user harus memasukkan password
        public bool RequirePassword { get; set; }

        // Method yang dipanggil saat halaman diakses dengan GET
        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User); // Ambil user saat ini
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Cek apakah user memiliki password (ada kemungkinan user login eksternal tanpa password)
            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        // Method yang dipanggil saat form di-submit dengan POST
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);

            // Jika password dibutuhkan, cek apakah password yang diinput valid
            if (RequirePassword)
            {
                bool isPasswordValid = await _userManager.CheckPasswordAsync(user, Input.Password);
                if (!isPasswordValid)
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password."); // Tambahkan error validasi
                    return Page(); // Kembali ke halaman dengan pesan error
                }
            }

            // Hapus user dari database
            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Unexpected error occurred deleting user.");
            }

            // Logout user setelah akun dihapus
            await _signInManager.SignOutAsync();

            // Logging aktivitas penghapusan akun
            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            // Redirect ke homepage setelah berhasil hapus akun
            return Redirect("~/");
        }
    }
}