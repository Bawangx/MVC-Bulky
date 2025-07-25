using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAccess.Data;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
    // Repository khusus untuk entitas ProductImage
    // Mewarisi class Repository umum yang meng-handle operasi dasar CRUD
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private ApplicationDbContext _db;

        // Konstruktor menerima ApplicationDbContext, diteruskan ke base class Repository
        public ProductImageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // Method untuk mengupdate data ProductImage
        public void Update(ProductImage obj)
        {
            // Entity Framework sudah menyediakan method Update untuk mengubah data entity
            // Pastikan SaveChanges() dipanggil di UnitOfWork agar perubahan tersimpan di database
            _db.ProductImages.Update(obj);
        }
    }
}