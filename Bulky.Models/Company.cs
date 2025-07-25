using System.ComponentModel.DataAnnotations;

namespace Bulky.Models
{
    // Model yang merepresentasikan data perusahaan (Company)
    public class Company
    {
        public int Id { get; set; }  // Primary key, otomatis unik dan untuk identifikasi data company

        [Required(ErrorMessage = "Name wajib diisi")]
        public string Name { get; set; }  // Nama perusahaan, wajib diisi

        public string? StreetAddress { get; set; }  // Alamat jalan, opsional (boleh kosong)

        public string? City { get; set; }  // Kota, opsional

        public string? State { get; set; }  // Provinsi/negara bagian, opsional

        public string? PostalCode { get; set; }  // Kode pos, opsional

        public string? PhoneNumber { get; set; }  // Nomor telepon, opsional
    }
}