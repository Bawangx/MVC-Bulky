// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// Static class yang berisi nama-nama halaman dan helper untuk menentukan kelas CSS aktif pada menu navigasi
    /// dalam halaman manajemen akun (Identity).
    /// </summary>
    public static class ManageNavPages
    {
        // Nama halaman manajemen akun (string) yang digunakan dalam routing dan penandaan aktif.
        public static string Index => "Index";
        public static string Email => "Email";
        public static string ChangePassword => "ChangePassword";
        public static string DownloadPersonalData => "DownloadPersonalData";
        public static string DeletePersonalData => "DeletePersonalData";
        public static string ExternalLogins => "ExternalLogins";
        public static string PersonalData => "PersonalData";
        public static string TwoFactorAuthentication => "TwoFactorAuthentication";

        // Metode helper untuk menentukan apakah halaman tertentu aktif,
        // sehingga bisa diberikan kelas CSS "active" untuk navigasi.
        // Parameter:
        //   viewContext: Konteks tampilan (ViewContext) dari Razor Page.
        //   page: Nama halaman yang ingin dicek.
        // Return:
        //   "active" jika halaman yang sedang aktif sama dengan halaman parameter, null jika tidak.
        public static string PageNavClass(ViewContext viewContext, string page)
        {
            // Mendapatkan nama halaman aktif dari ViewData["ActivePage"], jika null, ambil dari nama file halaman saat ini
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);

            // Bandingkan case-insensitive, jika sama kembalikan "active", jika tidak null
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }

        // Helper khusus untuk setiap halaman agar mudah dipanggil di Razor Page tanpa menuliskan nama halaman secara manual
        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
        public static string EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, Email);
        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);
        public static string DownloadPersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DownloadPersonalData);
        public static string DeletePersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DeletePersonalData);
        public static string ExternalLoginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExternalLogins);
        public static string PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);
        public static string TwoFactorAuthenticationNavClass(ViewContext viewContext) => PageNavClass(viewContext, TwoFactorAuthentication);
    }
}
