// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWeb.Areas.Identity.Pages
{
    /// <summary>
    /// Halaman model untuk menampilkan halaman error.
    /// Ini bagian dari infrastruktur default ASP.NET Core Identity.
    /// Biasanya tidak digunakan langsung oleh kode aplikasi Anda,
    /// tapi bisa dimodifikasi jika perlu.
    /// </summary>
    [AllowAnonymous] // Halaman ini bisa diakses tanpa login
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // Tidak menyimpan cache halaman error
    public class ErrorModel : PageModel
    {
        /// <summary>
        /// Menyimpan ID permintaan HTTP saat ini untuk tracking error
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Menentukan apakah RequestId tersedia untuk ditampilkan
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Dipanggil ketika halaman diakses via HTTP GET
        /// Mengisi nilai RequestId dengan ID aktivitas saat ini,
        /// jika tidak ada maka pakai TraceIdentifier dari HttpContext
        /// </summary>
        public void OnGet()
        {
            // Ambil Request ID dari aktivitas saat ini (jika ada),
            // kalau tidak ada ambil dari HttpContext
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}