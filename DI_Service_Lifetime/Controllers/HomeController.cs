using DI_Service_Lifetime.Models;
using DI_Service_Lifetime.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;

namespace DI_Service_Lifetime.Controllers
{
    public class HomeController : Controller
    {
        // Inject dua instance dari masing-masing service lifetime
        private readonly IScopedGuidService _scoped1;
        private readonly IScopedGuidService _scoped2;

        private readonly ISingletonGuidService _singleton1;
        private readonly ISingletonGuidService _singleton2;

        private readonly ITransientGuidService _transient1;
        private readonly ITransientGuidService _transient2;

        /// <summary>
        /// Constructor akan menerima dua instance dari masing-masing jenis service lifetime.
        /// Ini akan menunjukkan bagaimana .NET Core menangani lifetime yang berbeda.
        /// </summary>
        public HomeController(
            IScopedGuidService scoped1,
            IScopedGuidService scoped2,
            ISingletonGuidService singleton1,
            ISingletonGuidService singleton2,
            ITransientGuidService transient1,
            ITransientGuidService transient2)
        {
            _scoped1 = scoped1;
            _scoped2 = scoped2;
            _singleton1 = singleton1;
            _singleton2 = singleton2;
            _transient1 = transient1;
            _transient2 = transient2;
        }

        /// <summary>
        /// Endpoint utama untuk menampilkan hasil perbandingan GUID dari masing-masing service.
        /// </summary>
        public IActionResult Index()
        {
            StringBuilder messages = new StringBuilder();

            // Singleton: akan selalu sama, karena hanya dibuat satu kali seumur hidup aplikasi
            messages.AppendLine($"Singleton 1: {_singleton1.GetGuid()}");
            messages.AppendLine($"Singleton 2: {_singleton2.GetGuid()}\n");

            // Transient: selalu berbeda, karena instance baru dibuat setiap kali diminta
            messages.AppendLine($"Transient 1: {_transient1.GetGuid()}");
            messages.AppendLine($"Transient 2: {_transient2.GetGuid()}\n");

            // Scoped: sama dalam 1 request, tapi akan berbeda pada request yang lain
            messages.AppendLine($"Scoped 1: {_scoped1.GetGuid()}");
            messages.AppendLine($"Scoped 2: {_scoped2.GetGuid()}\n");

            return Ok(messages.ToString());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}