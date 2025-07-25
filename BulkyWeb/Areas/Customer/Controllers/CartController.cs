using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    // Menentukan area "customer" dan memastikan hanya user yang sudah login yang bisa akses controller ini
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;         // Unit of Work untuk akses database
        private readonly IEmailSender _emailSender;       // Untuk mengirim email notifikasi
        private readonly IConfiguration _configuration;   // Untuk baca konfigurasi (misal Stripe API key)

        // BindProperty agar model ini bisa otomatis menerima data dari form post
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        // Constructor, DI (Dependency Injection) untuk semua dependency yang dibutuhkan
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        // Menampilkan halaman keranjang belanja (cart)
        public IActionResult Index()
        {
            // Ambil UserId dari claim user yang sedang login
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Ambil daftar produk di keranjang user beserta data produk lengkapnya
            ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == userId,
                    includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            // Ambil semua gambar produk untuk menghubungkan ke tiap produk dalam cart
            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();

            // Loop tiap item di cart untuk isi gambar dan hitung total harga
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                // Assign list gambar untuk produk tersebut
                cart.Product.ProductImages = productImages.Where(pi => pi.ProductId == cart.Product.Id).ToList();

                // Hitung harga berdasarkan jumlah produk
                cart.Price = GetPriceBasedOnQuantity(cart);

                // Tambahkan ke total order
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        // Menampilkan halaman ringkasan pesanan sebelum checkout
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == userId,
                    includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            // Ambil data user untuk isi detail alamat dan kontak pada order header
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            // Salin data user ke order header
            var appUser = ShoppingCartVM.OrderHeader.ApplicationUser;
            ShoppingCartVM.OrderHeader.Name = appUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = appUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = appUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = appUser.City;
            ShoppingCartVM.OrderHeader.State = appUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = appUser.PostalCode;

            // Hitung total order dari harga dan jumlah produk di cart
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        // POST handler saat user klik checkout di halaman Summary
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Ambil data cart user lagi
            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            // Ambil data user
            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            // Hitung total order
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            // Jika user bukan perusahaan (CompanyId == 0), maka payment status pending
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else // Jika perusahaan, gunakan delayed payment dan status langsung approved
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            // Simpan order header ke database
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            // Simpan detail tiap produk yang dipesan
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                var orderDetail = new OrderDetail()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            // Jika user adalah customer biasa (bukan perusahaan), proses pembayaran via Stripe
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // Set Stripe API Key dari konfigurasi
                StripeConfiguration.ApiKey = _configuration.GetValue<string>("Stripe:SecretKey");

                var domain = $"{Request.Scheme}://{Request.Host.Value}/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                // Tambah tiap produk ke line items Stripe checkout
                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // Stripe menerima harga dalam sen
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);

                // Simpan ID session Stripe ke order header
                _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                // Redirect ke halaman pembayaran Stripe
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            // Jika user perusahaan, langsung redirect ke konfirmasi order
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        // Halaman konfirmasi order setelah checkout
        public IActionResult OrderConfirmation(int id)
        {
            // Ambil data order beserta data usernya
            var orderHeader = _unitOfWork.OrderHeader.Get(
                u => u.Id == id,
                includeProperties: "ApplicationUser");

            // Jika bukan pembayaran tertunda (delayed payment), cek status pembayaran Stripe
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                StripeConfiguration.ApiKey = _configuration.GetValue<string>("Stripe:SecretKey");

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                // Jika status pembayaran berhasil, update order status di DB
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }

                // Bersihkan session ASP.NET (cart session dll)
                HttpContext.Session.Clear();
            }

            // Kirim email notifikasi order ke user
            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book",
                $"<p>New Order Created - {orderHeader.Id}</p>");

            // Hapus semua item keranjang user karena sudah selesai order
            var shoppingCarts = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();

            return View(id);
        }

        // Menambah jumlah item di cart
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;

            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        // Mengurangi jumlah item di cart, jika tinggal 1 maka hapus itemnya
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);

            if (cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);

                // Update session cart count setelah hapus item
                int cartCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1;
                HttpContext.Session.SetInt32(SD.SessionCart, cartCount);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        // Menghapus item di cart
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);

            _unitOfWork.ShoppingCart.Remove(cartFromDb);

            // Update session cart count setelah hapus item
            int cartCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1;
            HttpContext.Session.SetInt32(SD.SessionCart, cartCount);

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        // Method pembantu untuk menentukan harga berdasarkan quantity produk
        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
                return shoppingCart.Product.Price;
            else if (shoppingCart.Count <= 100)
                return shoppingCart.Product.Price50;
            else
                return shoppingCart.Product.Price100;
        }
    }
}