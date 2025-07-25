using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bulky.Models
{
    public class ShoppingCart
    {
        // Primary key untuk tabel ShoppingCart
        public int Id { get; set; }

        // Foreign key ke Product
        public int ProductId { get; set; }

        // Navigasi ke entitas Product (relasi many-to-one)
        // Attribute ValidateNever artinya properti ini tidak akan divalidasi oleh model binding ASP.NET Core
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        // Jumlah produk yang ingin dibeli
        // Validasi agar jumlahnya antara 1 sampai 1000, jika tidak valid tampilkan pesan error
        [Range(1, 1000, ErrorMessage = "Please enter a value between 1 and 1000.")]
        public int Count { get; set; }

        // Foreign key ke ApplicationUser (user yang memiliki keranjang ini)
        public string ApplicationUserId { get; set; }

        // Navigasi ke entitas ApplicationUser (relasi many-to-one)
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        // Harga total untuk item ini (Count * Harga per produk)
        // NotMapped artinya properti ini tidak akan dibuat sebagai kolom di database
        [NotMapped]
        public double Price { get; set; }
    }
}