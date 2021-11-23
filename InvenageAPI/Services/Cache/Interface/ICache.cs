using InvenageAPI.Services.Enum;

namespace InvenageAPI.Services.Cache
{
    public interface ICache
    {
        public CacheType GetCacheType();

        public bool Get<T>(string key, out T result);

        public bool Set<T>(string key, T value, int expiresMinutes = 5);

        public bool Remove(string key);

        public bool Clear(long timeStamp, bool clearRemote = false);
    }
}
