using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAccess.Data;
using Bulky.Models;
using System;
using System.Linq;

namespace Bulky.DataAccess.Repository
{
    // Repository khusus untuk entitas OrderHeader,
    // mewarisi fungsi CRUD dasar dari Repository<T>
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;

        // Konstruktor menerima ApplicationDbContext dan diteruskan ke base class
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // Update data OrderHeader secara umum
        public void Update(OrderHeader obj)
        {
            // Tandai entity sebagai diupdate, perubahan disimpan saat SaveChanges dipanggil
            _db.OrderHeaders.Update(obj);
        }

        // Update status order dan optional status pembayaran
        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            // Cari data order berdasarkan id
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus; // Update status order
                if (paymentStatus != null)
                {
                    orderFromDb.PaymentStatus = paymentStatus; // Update status pembayaran jika ada
                }
            }
        }

        // Update informasi pembayaran Stripe dengan session dan paymentIntent
        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            // Pastikan sessionId tidak kosong
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.PaymentIntentId = paymentIntentId; // Simpan paymentIntentId dari Stripe
                orderFromDb.PaymentDate = DateTime.Now;         // Simpan waktu pembayaran sekarang
            }
        }
    }
}