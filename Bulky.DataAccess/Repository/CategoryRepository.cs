using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAccess.Data;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
    // Repository khusus untuk entitas Category,
    // mewarisi fungsi CRUD dasar dari Repository<T>
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;

        // Konstruktor menerima ApplicationDbContext dan meneruskannya ke base class
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // Method untuk meng-update data Category
        public void Update(Category obj)
        {
            // Tandai entity Category sebagai diupdate,
            // perubahan akan disimpan ketika SaveChanges() dipanggil
            _db.Categories.Update(obj);
        }
    }
}