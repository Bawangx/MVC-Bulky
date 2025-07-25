using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bulky.Models
{
    public class Product
    {
        [Key] // Menandakan properti ini sebagai primary key di database
        public int Id { get; set; }

        [Required] // Wajib diisi
        public string Title { get; set; }

        public string Description { get; set; } // Bisa kosong

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        [Display(Name = "List Price")] // Label tampilan untuk UI
        [Range(1, 10000, ErrorMessage = "List Price must be between 1 and 10000")] // Validasi range harga
        public double ListPrice { get; set; }

        [Required]
        [Display(Name = "Price For 1-50")]
        [Range(1, 10000, ErrorMessage = "Price must be between 1 and 10000")]
        public double Price { get; set; }

        [Required]
        [Display(Name = "Price For 50+")]
        [Range(1, 10000, ErrorMessage = "Price must be between 1 and 10000")]
        public double Price50 { get; set; }

        [Required]
        [Display(Name = "Price For 100+")]
        [Range(1, 10000, ErrorMessage = "Price must be between 1 and 10000")]
        public double Price100 { get; set; }

        // Foreign key untuk kategori produk
        public int CategoryId { get; set; }

        // Navigasi properti ke kategori, 
        // ValidateNever agar tidak divalidasi saat form submit (penting untuk properti navigasi)
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

        // List gambar produk, juga properti navigasi
        [ValidateNever]
        public List<ProductImage> ProductImages { get; set; }
    }
}