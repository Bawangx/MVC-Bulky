using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAccess.Data;
using Bulky.Models;
using System.Linq;

namespace Bulky.DataAccess.Repository
{
    // Repository khusus untuk entitas Product, turunan dari Repository umum
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;

        // Konstruktor menerima ApplicationDbContext, diteruskan ke base class Repository
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // Method untuk mengupdate data produk di database
        public void Update(Product obj)
        {
            // Cari produk di database berdasarkan Id-nya
            var objFromDb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);

            if (objFromDb != null)
            {
                // Update properti produk sesuai data dari parameter obj
                objFromDb.Title = obj.Title;
                objFromDb.Description = obj.Description;
                objFromDb.ISBN = obj.ISBN;
                objFromDb.Author = obj.Author;
                objFromDb.ListPrice = obj.ListPrice;
                objFromDb.Price = obj.Price;
                objFromDb.Price50 = obj.Price50;
                objFromDb.Price100 = obj.Price100;
                objFromDb.ProductImages = obj.ProductImages;

                // Update category jika CategoryId tidak nol
                if (obj.CategoryId != 0)
                {
                    objFromDb.CategoryId = obj.CategoryId;
                }

                // Jika ada properti lain yang ingin diupdate, bisa ditambahkan di sini
                // Contoh:
                // if (!string.IsNullOrEmpty(obj.ImageUrl))
                // {
                //     objFromDb.ImageUrl = obj.ImageUrl;
                // }
            }
            // Perlu diingat, perubahan baru akan tersimpan ke database setelah SaveChanges() dipanggil di UnitOfWork
        }
    }
}
