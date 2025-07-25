using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Bulky.Models.ViewModels
{
    // ViewModel untuk menggabungkan data Product dengan daftar kategori yang bisa dipilih
    public class ProductVM
    {
        // Properti Product yang berisi data produk
        public Product Product { get; set; }

        // Daftar kategori untuk dropdown di view (tidak divalidasi model binding)
        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }
}