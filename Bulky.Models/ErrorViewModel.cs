namespace Bulky.Models
{
    // Model untuk menampung informasi error yang bisa ditampilkan ke pengguna
    public class ErrorViewModel
    {
        // RequestId ini biasanya adalah ID unik yang merepresentasikan permintaan HTTP yang bermasalah
        // Bisa digunakan untuk melacak error di log aplikasi
        public string? RequestId { get; set; }

        // Property ini mengembalikan true jika RequestId tidak null atau kosong
        // Berguna untuk menentukan apakah RequestId harus ditampilkan di halaman error
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}