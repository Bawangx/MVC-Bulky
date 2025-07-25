using Bulky.Models;

namespace Bulky.DataAccess.Repository.IRepository
{
    // Interface untuk mengelola entitas Category
    // Mewarisi operasi dasar CRUD dari IRepository<Category>
    public interface ICategoryRepository : IRepository<Category>
    {
        // Method khusus untuk update data Category
        void Update(Category obj);
    }
}