using System.Diagnostics;
using System.Security.Claims;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        // Konstruktor untuk injeksi dependensi logger dan unit of work
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        // Halaman utama (Beranda)
        public IActionResult Index()
        {
            // Ambil ID user yang login, jika ada
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null)
            {
                var userId = userIdClaim.Value;

                // Hitung jumlah total item dalam cart user
                int cartCount = _unitOfWork.ShoppingCart
                    .GetAll(u => u.ApplicationUserId == userId)
                    .Sum(u => u.Count);

                // Simpan jumlah cart ke session
                HttpContext.Session.SetInt32(SD.SessionCart, cartCount);
            }

            // Ambil daftar semua produk, termasuk kategori mereka
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");

            // Tampilkan daftar produk di view
            return View(productList);
        }

        // Halaman detail produk
        public IActionResult Details(int productId)
        {
            // Siapkan cart default (untuk form tambah ke cart)
            ShoppingCart cart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages"),
                Count = 1,
                ProductId = productId
            };

            return View(cart);
        }

        // POST: Menambahkan produk ke keranjang
        [HttpPost]
        [Authorize] // Hanya user login yang boleh menambahkan ke cart
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            // Ambil ID user login
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                // Jika user belum login, redirect ke halaman login
                return RedirectToAction("Login", "Account");
            }

            shoppingCart.ApplicationUserId = userId;

            // Cek apakah produk sudah ada di keranjang sebelumnya
            var cartFromDb = _unitOfWork.ShoppingCart.Get(
                u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

            if (cartFromDb == null)
            {
                // Jika belum ada, tambahkan ke database
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }
            else
            {
                // Jika sudah ada, tambahkan quantity-nya
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();

            // Update jumlah item di session
            int cartCount = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == userId)
                .Sum(u => u.Count);

            HttpContext.Session.SetInt32(SD.SessionCart, cartCount);

            // TempData menampilkan notifikasi satu kali setelah redirect
            TempData["success"] = "Cart updated successfully";

            // Redirect kembali ke halaman utama
            return RedirectToAction(nameof(Index));
        }

        // Halaman Privacy Policy
        public IActionResult Privacy()
        {
            return View();
        }

        // Halaman Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}