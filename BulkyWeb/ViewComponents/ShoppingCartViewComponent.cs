using Bulky.DataAccess.Repository.IRepository;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.ViewComponents
{
    // ViewComponent untuk menampilkan jumlah item di Shopping Cart pada navbar atau halaman lain
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        // Constructor Dependency Injection untuk mendapatkan akses ke database melalui UnitOfWork
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Method yang dipanggil secara async saat ViewComponent dipanggil di view
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Ambil identity user yang sedang login
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            // Cari claim untuk user identifier (ID user)
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null) // Jika user sudah login
            {
                // Cek apakah session cart sudah ada
                if (HttpContext.Session.GetInt32(SD.SessionCart) == null)
                {
                    // Jika belum, hitung jumlah item di shopping cart dari database untuk user ini
                    var cartCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Count();

                    // Simpan jumlah item di session agar tidak perlu query berulang kali
                    HttpContext.Session.SetInt32(SD.SessionCart, cartCount);
                }

                // Ambil jumlah item dari session dan kirim ke view
                var sessionCartCount = HttpContext.Session.GetInt32(SD.SessionCart);

                // Return view dengan model jumlah item (nullable int, bisa null)
                return View(sessionCartCount);
            }
            else // Jika user belum login
            {
                // Kosongkan session karena tidak ada user
                HttpContext.Session.Clear();

                // Tampilkan 0 karena tidak ada item di keranjang
                return View(0);
            }
        }
    }
}