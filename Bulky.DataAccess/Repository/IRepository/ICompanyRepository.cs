using Bulky.Models;

namespace Bulky.DataAccess.Repository.IRepository
{
    // Interface untuk mengelola data entitas Company
    // Mewarisi operasi dasar CRUD dari IRepository<Company>
    public interface ICompanyRepository : IRepository<Company>
    {
        // Method khusus untuk update data Company
        void Update(Company obj);
    }
}