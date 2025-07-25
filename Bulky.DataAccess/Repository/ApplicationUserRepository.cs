using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
    // Repository khusus untuk entitas ApplicationUser (user aplikasi)
    // Mewarisi fungsi CRUD dasar dari Repository<T>
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;

        // Konstruktor menerima ApplicationDbContext dan meneruskannya ke base class
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // Method untuk meng-update data ApplicationUser
        public void Update(ApplicationUser applicationUser)
        {
            // Tandai entity ApplicationUser sebagai diupdate,
            // perubahan akan disimpan ketika SaveChanges() dipanggil
            _db.ApplicationUsers.Update(applicationUser);
        }
    }
}