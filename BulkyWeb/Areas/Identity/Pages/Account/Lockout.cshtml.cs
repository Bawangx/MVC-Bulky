// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Page model untuk halaman Lockout.
    /// Halaman ini akan ditampilkan saat akun pengguna terkunci (locked out).
    /// [AllowAnonymous] berarti halaman ini dapat diakses tanpa perlu login terlebih dahulu.
    /// </summary>
    [AllowAnonymous]
    public class LockoutModel : PageModel
    {
        /// <summary>
        /// Method yang dipanggil ketika halaman diakses via GET request.
        /// Karena halaman ini hanya menampilkan informasi statis, method ini kosong.
        /// </summary>
        public void OnGet()
        {
            // Tidak perlu logic khusus saat load halaman lockout
        }
    }
}