using Bulky.Models;

namespace Bulky.DataAccess.Repository.IRepository
{
    // Interface untuk operasi pada entitas OrderHeader
    // Mewarisi operasi dasar CRUD dari IRepository<OrderHeader>
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        // Method untuk update seluruh data OrderHeader
        void Update(OrderHeader obj);

        // Method untuk mengubah status order dan opsional status pembayaran
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);

        // Method khusus update informasi pembayaran dari Stripe
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
    }
}