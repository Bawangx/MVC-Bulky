using System;

namespace Bulky.Utility
{
    // Kelas static ini berfungsi sebagai tempat menyimpan nilai-nilai konstan
    // yang sering digunakan di aplikasi, misalnya nama role, status order, dll.
    public static class SD
    {
        // Role pengguna dalam aplikasi
        public const string Role_Customer = "Customer";   // Pelanggan
        public const string Role_Company = "Company";     // Perusahaan
        public const string Role_Admin = "Admin";         // Administrator
        public const string Role_Employee = "Employee";   // Karyawan

        // Status order
        public const string StatusPending = "Pending";       // Menunggu proses
        public const string StatusApproved = "Approved";     // Disetujui
        public const string StatusInProcess = "InProcess";   // Sedang diproses
        public const string StatusShipped = "Shipped";       // Sudah dikirim
        public const string StatusCancelled = "Cancelled";   // Dibatalkan
        public const string StatusRefunded = "Refunded";     // Sudah direfund

        // Status pembayaran
        public const string PaymentStatusPending = "Pending";          // Pembayaran menunggu
        public const string PaymentStatusApproved = "Approved";        // Pembayaran disetujui
        public const string PaymentStatusDelayedPayment = "DelayedPayment";  // Pembayaran tertunda
        public const string PaymentStatusRejected = "Rejected";        // Pembayaran ditolak

        // Nama session untuk keranjang belanja (shopping cart)
        public const string SessionCart = "SessionShoppingCart";
    }
}