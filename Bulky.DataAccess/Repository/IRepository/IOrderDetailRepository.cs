using Bulky.Models;

namespace Bulky.DataAccess.Repository.IRepository
{
    // Interface untuk operasi pada entitas OrderDetail
    // Mewarisi operasi dasar CRUD dari IRepository<OrderDetail>
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        // Method untuk update data OrderDetail secara spesifik
        void Update(OrderDetail obj);
    }
}