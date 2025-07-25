using Bulky.DataAccess.Repository.IRepository;
using Bulky.DataAccess.Data;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
    // Repository khusus untuk entitas Company,
    // mewarisi fungsi CRUD dasar dari Repository<T>
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private ApplicationDbContext _db;

        // Konstruktor menerima ApplicationDbContext dan meneruskannya ke base class
        public CompanyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        // Method untuk meng-update data Company
        public void Update(Company obj)
        {
            // Tandai entity Company sebagai diupdate,
            // perubahan akan disimpan ketika SaveChanges() dipanggil
            _db.Companies.Update(obj);
        }
    }
}