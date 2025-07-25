using System;

namespace DI_Service_Lifetime.Services
{
    /// <summary>
    /// Layanan Transient yang menghasilkan GUID (Global Unique Identifier).
    /// Instance baru akan dibuat setiap kali service ini diminta.
    /// </summary>
    public class TransientGuidService : ITransientGuidService
    {
        // Menyimpan GUID yang dibuat saat instance ini dibuat
        private readonly Guid _id;

        /// <summary>
        /// Konstruktor akan dipanggil setiap kali service ini di-inject.
        /// Artinya, setiap permintaan akan mendapatkan GUID baru.
        /// </summary>
        public TransientGuidService()
        {
            _id = Guid.NewGuid(); // Membuat GUID baru
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