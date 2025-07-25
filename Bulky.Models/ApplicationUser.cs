using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bulky.Models
{
    // Kelas ApplicationUser mewarisi IdentityUser dari ASP.NET Core Identity
    // untuk menambahkan properti tambahan pada user (seperti profil)
    public class ApplicationUser : IdentityUser
    {
        // Nama lengkap user
        public string Name { get; set; }

        // Alamat lengkap user (opsional, bisa null)
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        // Jika user terkait dengan perusahaan, simpan CompanyId (nullable)
        public int? CompanyId { get; set; }

        // Relasi ke objek Company, diabaikan saat validasi model
        [ForeignKey("CompanyId")]
        [ValidateNever]
        public Company? Company { get; set; }

        // Properti Role tidak disimpan di database ([NotMapped])
        // Biasanya digunakan untuk menyimpan role user secara sementara saat runtime
        [NotMapped]
        public string Role { get; set; }
    }
}