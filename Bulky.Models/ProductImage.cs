using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bulky.Models
{
    public class ProductImage
    {
        // Primary key untuk tabel ProductImage
        public int Id { get; set; }

        // URL gambar produk, wajib diisi (Required)
        [Required]
        public string ImageUrl { get; set; }

        // Foreign key untuk menghubungkan gambar dengan produk
        public int ProductId { get; set; }

        // Properti navigasi ke entitas Product
        // Menandakan hubungan many-to-one: satu produk bisa punya banyak gambar
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}