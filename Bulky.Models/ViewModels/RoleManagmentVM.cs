using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
    public class RoleManagmentVM
    {
        public ApplicationUser ApplicationUser { get; set; } // User yang akan dikelola role-nya
        public IEnumerable<SelectListItem> RoleList { get; set; } // Daftar nama role yang dimiliki user
        public IEnumerable<SelectListItem> CompanyList { get; set; } // Semua role yang tersedia di sistem
    }
}
