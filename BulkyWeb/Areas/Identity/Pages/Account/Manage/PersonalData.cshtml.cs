// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BulkyWeb.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// PageModel untuk mengelola data pribadi pengguna, seperti menampilkan halaman pengelolaan data pribadi.
    /// </summary>
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PersonalDataModel> _logger;

        // Constructor menerima dependensi UserManager dan Logger yang diperlukan untuk operasi pengguna dan logging
        public PersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<PersonalDataModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Menangani request GET ke halaman ini.
        /// Memastikan pengguna yang sedang login dapat dimuat, jika tidak ditemukan kembalikan NotFound.
        /// </summary>
        /// <returns>Halaman jika berhasil, atau error jika user tidak ditemukan</returns>
        public async Task<IActionResult> OnGet()
        {
            // Mendapatkan user saat ini berdasarkan context User
            var user = await _userManager.GetUserAsync(User);

            // Jika user tidak ditemukan, kembalikan NotFound
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Jika berhasil, tampilkan halaman
            return Page();
        }
    }
}