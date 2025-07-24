namespace DI_Service_Lifetime.Services
{
    public class TransientGuidService : ITransientGuidService
    {
        private readonly Guid _id;

        public TransientGuidService()
        {
            _id = Guid.NewGuid();
        }

        public string GetGuid()
        {
            return _id.ToString();
        }
    }
}
