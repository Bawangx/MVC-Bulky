using Bulky.Models;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    // Interface khusus untuk operasi pada entitas Product
    // Meng-extend IRepository<Product> sehingga mewarisi semua method dasar CRUD
    public interface IProductRepository : IRepository<Product>
    {
        // Menambahkan method Update khusus untuk entity Product
        void Update(Product obj);
    }
}