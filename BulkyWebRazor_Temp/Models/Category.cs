using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyWebRazor_Temp.Models
{
    public class Category
    {
        // Primary key untuk entity Category
        [Key]
        public int Id { get; set; }

        // Nama kategori wajib diisi, maksimal 30 karakter
        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(30, ErrorMessage = "Category name cannot exceed 30 characters")]
        [DisplayName("Category Name")] // Label yang muncul di UI form
        public string Name { get; set; }

        // Urutan tampil kategori, harus antara 1 sampai 100
        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order Must Be Between 1-100")]
        public int DisplayOrder { get; set; }
    }
}