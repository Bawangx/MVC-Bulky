namespace DI_Service_Lifetime.Services
{
    // Interface untuk layanan dengan lifetime "Singleton"
    // Singleton berarti instance layanan ini dibuat satu kali dan digunakan sepanjang aplikasi berjalan.
    public interface ISingletonGuidService
    {
        // Method untuk mendapatkan GUID dalam bentuk string
        string GetGuid();
    }
}