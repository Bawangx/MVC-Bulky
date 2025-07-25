using System;

namespace DI_Service_Lifetime.Services
{
    /// <summary>
    /// Layanan Singleton yang menghasilkan GUID (Global Unique Identifier).
    /// GUID hanya dibuat sekali seumur hidup aplikasi (selama aplikasi berjalan).
    /// </summary>
    public class SingletonGuidService : ISingletonGuidService
    {
        // Menyimpan GUID yang dibuat saat instance ini pertama kali dibuat
        private readonly Guid _id;

        /// <summary>
        /// Konstruktor dipanggil hanya satu kali selama siklus hidup aplikasi.
        /// GUID akan tetap sama selama aplikasi belum dimatikan atau direstart.
        /// </summary>
        public SingletonGuidService()
        {
            _id = Guid.NewGuid(); // Membuat GUID baru saat service pertama kali dibuat
        }

        /// <summary>
        /// Mengembalikan GUID dalam bentuk string.
        /// </summary>
        /// <returns>GUID sebagai string</returns>
        public string GetGuid()
        {
            return _id.ToString();
        }
    }
}