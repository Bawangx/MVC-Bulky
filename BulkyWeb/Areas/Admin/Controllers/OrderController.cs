using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize] // Pastikan user login untuk mengakses controller ini
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        // Property untuk menyimpan data OrderVM yang di-bind secara otomatis dari form post
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Menampilkan halaman daftar order (data akan dimuat via AJAX)
        public IActionResult Index()
        {
            return View();
        }

        // Menampilkan detail order berdasarkan orderId
        public IActionResult Details(int orderId)
        {
            // Mengambil header dan detail order sekaligus dengan relasi yang dibutuhkan
            OrderVM = new OrderVM
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        // Update detail order, hanya boleh diakses oleh Admin dan Employee
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            // Ambil data order dari DB berdasarkan Id yang diterima dari form
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);

            if (orderHeaderFromDb == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            // Update data pengiriman sesuai input form
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

            if (!string.IsNullOrWhiteSpace(OrderVM.OrderHeader.Carrier))
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;

            if (!string.IsNullOrWhiteSpace(OrderVM.OrderHeader.TrackingNumber))
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully";

            // Kembali ke halaman detail order
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        // Menandai order sebagai sudah dikirim
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);

            if (orderHeader == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            // Update info pengiriman dan status order
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            // Jika pembayaran dengan metode delayed payment, set jatuh tempo pembayaran
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueData = DateTime.Now.AddDays(30);
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            TempData["Success"] = "Order Shipped Successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderHeader.Id });
        }

        // Membatalkan order, dan jika sudah bayar akan lakukan refund via Stripe
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);

            if (orderHeader == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                // Buat refund pada Stripe
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                service.Create(options);

                // Update status order menjadi cancelled dan refunded
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                // Jika belum bayar, cukup update status cancel saja
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            _unitOfWork.Save();

            TempData["Success"] = "Order Cancelled Successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderHeader.Id });
        }

        // Proses pembayaran via Stripe (membuat session checkout Stripe)
        [HttpPost]
        [ActionName("Details")]
        public IActionResult Details_PAY_NOW()
        {
            // Ambil data order beserta detail produk
            OrderVM.OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

            // URL domain harus disesuaikan dengan domain produksi (bukan localhost)
            var domain = "https://localhost:7154/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment"
            };

            // Menambahkan produk ke line items Stripe checkout
            foreach (var item in OrderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // Stripe pakai cents
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

            // Membuat session Stripe checkout
            var service = new SessionService();
            Session session = service.Create(options);

            // Simpan Stripe session Id dan paymentIntent Id ke DB untuk tracking
            _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            // Redirect ke halaman pembayaran Stripe
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        // Konfirmasi pembayaran Stripe dan update status order
        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    // Update status pembayaran dan simpan session baru jika ada perubahan
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            return View(orderHeaderId);
        }

        #region API CALLS

        // API untuk mengambil data order dengan filter status dan role user
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;

            // Jika Admin atau Employee, dapat akses semua order
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                // Jika user biasa, hanya dapat lihat order miliknya sendiri
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaders = _unitOfWork.OrderHeader
                    .GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            // Filter order berdasarkan status yang dikirim via query string
            objOrderHeaders = status switch
            {
                "pending" => objOrderHeaders.Where(u => u.OrderStatus == SD.PaymentStatusDelayedPayment),
                "inprocess" => objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess),
                "completed" => objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped),
                "approved" => objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved),
                _ => objOrderHeaders
            };

            return Json(new { data = objOrderHeaders.ToList() });
        }

        #endregion
    }
}