using System.Collections.Generic;

namespace Bulky.Models.ViewModels
{
    // ViewModel untuk menggabungkan data header order dan detail order
    public class OrderVM
    {
        // Data utama tentang order (informasi umum seperti tanggal, status, total)
        public OrderHeader OrderHeader { get; set; }

        // Daftar detail order yang berisi produk dan jumlahnya di dalam order
        public IEnumerable<OrderDetail> OrderDetail { get; set; }
    }
}