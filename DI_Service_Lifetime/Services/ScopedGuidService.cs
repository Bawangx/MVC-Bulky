namespace DI_Service_Lifetime.Services
{
    // Implementasi service dengan lifetime Scoped
    // Scoped artinya instance service ini dibuat sekali untuk setiap request HTTP
    // Jadi selama satu request HTTP, instance ini akan tetap sama
    public class ScopedGuidService : IScopedGuidService
    {
        // Menyimpan GUID unik untuk instance ini
        private readonly Guid _id;

        // Konstruktor dipanggil sekali saat instance dibuat
        public ScopedGuidService()
        {
            // Membuat GUID baru setiap instance service dibuat
            _id = Guid.NewGuid();
        }

        // Mengembalikan GUID sebagai string
        public string GetGuid()
        {
            return _id.ToString();
        }
    }
}