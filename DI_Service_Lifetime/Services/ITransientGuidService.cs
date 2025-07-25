namespace DI_Service_Lifetime.Services
{
    // Interface untuk layanan dengan lifetime "Transient"
    // Transient berarti setiap kali service diminta, instance baru akan dibuat
    public interface ITransientGuidService
    {
        // Method untuk mendapatkan GUID dalam bentuk string
        string GetGuid();
    }
}