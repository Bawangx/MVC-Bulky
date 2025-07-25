using Bulky.Models;

namespace Bulky.DataAccess.Repository.IRepository
{
    // Interface untuk mengelola entitas ApplicationUser
    // Mewarisi operasi dasar CRUD dari IRepository<ApplicationUser>
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        // Method khusus untuk update data ApplicationUser
        void Update(ApplicationUser applicationUser);
    }
}