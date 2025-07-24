// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWeb.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// Halaman Razor Page untuk mengatur password baru pengguna yang belum memiliki password lokal.
    /// Biasanya digunakan jika pengguna sebelumnya login menggunakan external login (Google, Facebook, dll).
    /// </summary>
    public class SetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;       // Untuk mengelola user, password, dll
        private readonly SignInManager<ApplicationUser> _signInManager;   // Untuk mengelola sesi login pengguna

        public SetPasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Property yang di-bind ke form input pada halaman.
        /// Menerima data password baru dan konfirmasi password.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Menyimpan pesan status (sukses/error) yang bisa ditampilkan pada halaman menggunakan TempData.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Model untuk input form set password dengan validasi data menggunakan DataAnnotations.
        /// </summary>
        public class InputModel
        {
            [Required(ErrorMessage = "Password baru harus diisi.")]
            [StringLength(100, ErrorMessage = "Password harus terdiri dari minimal {2} dan maksimal {1} karakter.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "Password konfirmasi tidak cocok dengan password baru.")]
            public string ConfirmPassword { get; set; }
        }

        /// <summary>
        /// Handler untuk request GET ke halaman ini.
        /// Mengecek apakah pengguna sudah punya password.
        /// Jika sudah, diarahkan ke halaman ganti password.
        /// Jika belum, tetap di halaman set password.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // Ambil user yang sedang login
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Jika user tidak ditemukan, tampilkan halaman 404
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Cek apakah user sudah punya password
            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                // Jika sudah ada password, redirect ke halaman ChangePassword
                return RedirectToPage("./ChangePassword");
            }

            // Jika belum punya password, tampilkan halaman ini
            return Page();
        }

        /// <summary>
        /// Handler untuk request POST ketika form disubmit.
        /// Memvalidasi input, lalu menambahkan password ke user.
        /// Jika berhasil, refresh sign-in dan tampilkan pesan sukses.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // Validasi model input, jika tidak valid tampilkan ulang halaman dengan error
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Ambil user yang sedang login
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Jika user tidak ditemukan, tampilkan halaman 404
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Tambahkan password baru ke user
            var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                // Jika gagal, masukkan pesan error ke ModelState agar tampil di halaman
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Refresh sesi login agar update password diakui sistem
            await _signInManager.RefreshSignInAsync(user);
            // Simpan pesan sukses ke TempData agar bisa ditampilkan setelah redirect
            StatusMessage = "Your password has been set.";

            // Redirect ke halaman ini kembali untuk mencegah refresh form POST (PRG pattern)
            return RedirectToPage();
        }
    }
}