using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Bulky.Models.ViewModels
{
    // ViewModel untuk manajemen Role pengguna (ApplicationUser)
    public class RoleManagmentVM
    {
        // Objek ApplicationUser yang sedang dikelola rolenya
        public ApplicationUser ApplicationUser { get; set; }

        // Daftar Role yang dapat dipilih untuk pengguna ini
        // Digunakan untuk dropdown/select di view
        public IEnumerable<SelectListItem> RoleList { get; set; }

        // Daftar Company yang bisa dipilih, biasanya untuk mengaitkan user dengan company
        public IEnumerable<SelectListItem> CompanyList { get; set; }
    }
}