using InvenageAPI.Services.Cache;
using InvenageAPI.Services.Connection;
using InvenageAPI.Services.Dependent;
using InvenageAPI.Services.Storage;
using InvenageAPI.Services.Synchronizer;
using Microsoft.Extensions.DependencyInjection;

namespace InvenageAPI.Services.Extension
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDependents(this IServiceCollection service)
        {
            return service
                .AddSingleton<ILocalCache, LocalCache>()
                .AddSingleton<IRemoteCache, RemoteCache>()
                .AddSingleton<ILocalStorage, LocalStorage>()
                .AddSingleton<IRemoteStorage, RemoteStorage>()
                .AddSingleton<IArchiveStorage, ArchiveStorage>()
                .AddSingleton<IApiConnection, ApiConnection>()
                .AddSingleton<ICacheSynchronizer, CacheSynchronizer>()
                .AddSingleton<IStorageSynchronizer, StorageSynchronizer>()
                .AddSingleton<IDependent, DependentServices>();
        }
    }
}
