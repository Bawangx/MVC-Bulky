using System;

namespace Bulky.DataAccess.Repository.IRepository
{
    // Interface Unit of Work untuk mengelola repository secara terpusat
    public interface IUnitOfWork
    {
        // Properti untuk masing-masing repository yang digunakan di aplikasi
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IOrderDetailRepository OrderDetail { get; }
        IProductImageRepository ProductImage { get; }

        // Method untuk menyimpan perubahan (commit) ke database sekaligus
        void Save();
    }
}