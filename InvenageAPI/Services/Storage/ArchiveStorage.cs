using Microsoft.Extensions.Configuration;

namespace InvenageAPI.Services.Storage
{
    public class ArchiveStorage : RemoteStorage, IArchiveStorage
    {
        public ArchiveStorage(IConfiguration config) : base(config)
        {
            storageType = "Archive";
            connectionString = config.GetConnectionString("ArchiveStorage");
        }
    }
}