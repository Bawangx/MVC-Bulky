using Bulky.Models;

namespace Bulky.DataAccess.Repository.IRepository
{
    // Interface untuk operasi pada entitas ProductImage
    // Mewarisi operasi dasar CRUD dari IRepository<ProductImage>
    public interface IProductImageRepository : IRepository<ProductImage>
    {
        // Method khusus untuk update ProductImage
        void Update(ProductImage obj);
    }
}