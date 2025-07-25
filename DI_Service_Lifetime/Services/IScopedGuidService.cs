namespace DI_Service_Lifetime.Services
{
    // Interface untuk layanan dengan lifetime "Scoped"
    // Scoped berarti instance layanan ini akan sama selama satu request HTTP,
    // tapi berbeda untuk request yang berbeda.
    public interface IScopedGuidService
    {
        // Method untuk mendapatkan GUID dalam bentuk string
        string GetGuid();
    }
}