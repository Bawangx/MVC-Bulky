using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAccess.Data;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
    // Repository khusus untuk entitas OrderDetail,
    // mewarisi fungsi CRUD dasar dari Repository<T>
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDbContext _db;

        // Konstruktor menerima ApplicationDbContext dan diteruskan ke base class
        public OrderDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // Method untuk update data OrderDetail
        public void Update(OrderDetail obj)
        {
            // Tandai entity OrderDetail sebagai diupdate,
            // perubahan akan disimpan ketika SaveChanges dipanggil
            _db.OrderDetails.Update(obj);
        }
    }
}