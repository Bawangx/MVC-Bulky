// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    /// <summary>
    /// PageModel untuk halaman Access Denied.
    /// Halaman ini ditampilkan ketika pengguna mencoba mengakses resource yang tidak mereka miliki izin.
    /// </summary>
    public class AccessDeniedModel : PageModel
    {
        /// <summary>
        /// Method yang dipanggil saat HTTP GET request ke halaman ini.
        /// Karena halaman ini statis dan hanya menampilkan pesan, method ini kosong.
        /// </summary>
        public void OnGet()
        {
            // Tidak ada logika khusus diperlukan untuk halaman ini.
        }
    }
}