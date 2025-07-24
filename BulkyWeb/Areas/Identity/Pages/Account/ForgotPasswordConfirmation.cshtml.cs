// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWeb.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Halaman ini menampilkan konfirmasi bahwa email untuk reset password sudah dikirim.
    /// Halaman ini dapat diakses tanpa perlu login ([AllowAnonymous]).
    /// </summary>
    [AllowAnonymous]
    public class ForgotPasswordConfirmation : PageModel
    {
        /// <summary>
        /// Method ini dipanggil ketika halaman diakses dengan metode GET.
        /// Tidak ada logika khusus pada halaman ini, hanya menampilkan halaman statis.
        /// </summary>
        public void OnGet()
        {
            // Tidak perlu kode khusus, hanya menampilkan halaman konfirmasi.
        }
    }
}