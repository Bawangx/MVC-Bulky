using System.Collections.Generic;

namespace Bulky.Models.ViewModels
{
    // ViewModel untuk menggabungkan data ShoppingCart dan OrderHeader
    // Digunakan untuk menampilkan data keranjang belanja dan info order secara bersamaan di View
    public class ShoppingCartVM
    {
        // Koleksi item dalam keranjang belanja
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }

        // Informasi header pesanan (seperti total harga, data pemesan, dll)
        public OrderHeader OrderHeader { get; set; }
    }
}