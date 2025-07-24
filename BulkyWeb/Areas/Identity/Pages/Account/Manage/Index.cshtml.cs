// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWeb.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Constructor inject UserManager dan SignInManager dari Identity
        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Username pengguna yang akan ditampilkan (readonly).
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Pesan status operasi, ditampilkan menggunakan TempData agar bisa survive redirect.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Model yang mengikat input form, dalam hal ini hanya PhoneNumber.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Model input untuk form profil, hanya mengandung nomor telepon.
        /// </summary>
        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        /// <summary>
        /// Memuat data pengguna dari database ke properti model.
        /// </summary>
        /// <param name="user">Objek ApplicationUser yang sudah didapat dari UserManager</param>
        private async Task LoadAsync(ApplicationUser user)
        {
            // Ambil username dan nomor telepon saat ini
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }

        /// <summary>
        /// Handler GET, untuk menampilkan data profil awal ke user.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // Cari user yang sedang login
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Jika user tidak ditemukan, tampilkan 404
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Load data user ke model
            await LoadAsync(user);
            return Page();
        }

        /// <summary>
        /// Handler POST, untuk menerima dan memproses update profil user.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Jika user tidak ditemukan, tampilkan 404
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Validasi model input
            if (!ModelState.IsValid)
            {
                // Jika validasi gagal, reload data user agar tetap muncul di form
                await LoadAsync(user);
                return Page();
            }

            // Ambil nomor telepon lama
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            // Jika nomor telepon baru berbeda dengan yang lama, update
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    // Jika gagal, set pesan error dan redirect ulang ke halaman
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // Refresh session sign-in agar info terbaru terupdate pada cookie
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}