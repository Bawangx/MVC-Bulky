// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Bulky.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace BulkyWeb.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager; // Untuk mengelola user
        private readonly ILogger<DownloadPersonalDataModel> _logger; // Untuk logging aktivitas

        // Constructor untuk dependency injection UserManager dan Logger
        public DownloadPersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<DownloadPersonalDataModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // Handle request GET ke halaman ini, kita blok dengan NotFound (404) karena hanya POST yang valid
        public IActionResult OnGet()
        {
            return NotFound();
        }

        // Handle POST, yaitu saat user meminta data pribadinya untuk diunduh
        public async Task<IActionResult> OnPostAsync()
        {
            // Ambil user yang sedang login
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Jika user tidak ditemukan, kembalikan 404
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Log aktivitas user yang request data pribadinya
            _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

            // Buat dictionary untuk menyimpan data pribadi user yang akan diunduh
            var personalData = new Dictionary<string, string>();

            // Dapatkan properti dari ApplicationUser yang memiliki atribut [PersonalData]
            var personalDataProps = typeof(ApplicationUser).GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

            // Masukkan data properti tersebut ke dictionary, jika null diberi string "null"
            foreach (var prop in personalDataProps)
            {
                personalData.Add(prop.Name, prop.GetValue(user)?.ToString() ?? "null");
            }

            // Ambil juga external login yang terdaftar, misal login lewat Google, Facebook, dll
            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var login in logins)
            {
                personalData.Add($"{login.LoginProvider} external login provider key", login.ProviderKey);
            }

            // Sertakan juga Authenticator Key untuk 2FA
            personalData.Add("Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));

            // Atur header agar browser menganggap response ini sebagai file yang harus didownload
            Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");

            // Kembalikan file JSON berisi data pribadi user
            return new FileContentResult(
                JsonSerializer.SerializeToUtf8Bytes(personalData),
                "application/json");
        }
    }
}