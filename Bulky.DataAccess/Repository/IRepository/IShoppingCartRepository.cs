using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    // Interface khusus untuk ShoppingCart yang mewarisi interface umum IRepository
    // Menambahkan method Update yang khusus untuk ShoppingCart
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        // Method untuk mengupdate data ShoppingCart
        void Update(ShoppingCart obj);
    }
}