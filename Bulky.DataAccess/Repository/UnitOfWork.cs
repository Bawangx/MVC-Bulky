using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAccess.Data;

namespace Bulky.DataAccess.Repository
{
    // UnitOfWork adalah pola desain yang menggabungkan semua repository dalam satu kelas.
    // Ini memudahkan pengelolaan transaksi database agar lebih terorganisir dan konsisten.
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        // Properti untuk mengakses repository masing-masing entitas
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public IProductImageRepository ProductImage { get; private set; }

        // Konstruktor menerima DbContext untuk dipakai di semua repository
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;

            // Inisialisasi semua repository dengan DbContext yang sama
            ProductImage = new ProductImageRepository(_db);
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);
            OrderDetail = new OrderDetailRepository(_db);
        }

        // Metode untuk menyimpan perubahan ke database secara keseluruhan (commit)
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}