using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAccess.Data;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
    // ShoppingCartRepository mewarisi Repository umum untuk entitas ShoppingCart,
    // dan menambahkan metode khusus bila diperlukan
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;

        // Konstruktor menerima ApplicationDbContext dan meneruskannya ke kelas dasar (Repository)
        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // Metode khusus untuk update objek ShoppingCart di database
        public void Update(ShoppingCart obj)
        {
            // Update entitas ShoppingCart di DbContext
            _db.ShoppingCarts.Update(obj);
        }
    }
}