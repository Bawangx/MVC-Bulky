// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Halaman model untuk menampilkan konfirmasi bahwa password telah berhasil di-reset.
    /// Bagian dari infrastruktur ASP.NET Core Identity default UI.
    /// Biasanya tidak diubah, tapi bisa dikustomisasi jika perlu.
    /// </summary>
    [AllowAnonymous] // Mengizinkan akses tanpa login
    public class ResetPasswordConfirmationModel : PageModel
    {
        /// <summary>
        /// Method yang dipanggil saat halaman diakses dengan HTTP GET.
        /// Karena halaman ini hanya menampilkan pesan konfirmasi,
        /// method ini tidak perlu melakukan apa-apa.
        /// </summary>
        public void OnGet()
        {
            // Tidak ada logika yang perlu dijalankan saat GET
        }
    }
}