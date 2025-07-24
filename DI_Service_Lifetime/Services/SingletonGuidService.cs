namespace DI_Service_Lifetime.Services
{
    public class SingletonGuidService : ISingletonGuidService
    {
        private readonly Guid _id;

        public SingletonGuidService()
        {
            _id = Guid.NewGuid();
        }

        public string GetGuid()
        {
            return _id.ToString();
        }
    }
}
