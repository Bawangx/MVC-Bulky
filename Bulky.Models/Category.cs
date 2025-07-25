using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bulky.Models
{
    // Model untuk kategori produk atau entitas lain yang membutuhkan kategori
    public class Category
    {
        [Key] // Menandakan properti ini sebagai primary key di database
        public int Id { get; set; }

        [Required(ErrorMessage = "Name wajib diisi")] // Nama kategori harus diisi
        [MaxLength(30, ErrorMessage = "Name maksimal 30 karakter")] // Maksimal 30 karakter
        [DisplayName("Category Name")] // Label yang akan tampil di UI (misal form)
        public string Name { get; set; }

        [DisplayName("Display Order")] // Label untuk UI
        [Range(1, 100, ErrorMessage = "Display Order harus antara 1 sampai 100")] // Validasi nilai minimal dan maksimal
        public int DisplayOrder { get; set; }
    }
}