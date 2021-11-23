namespace InvenageAPI.Services.Synchronizer
{
    public interface ICacheSynchronizer : ISynchronizer
    {
        public void Queue<T>(string key, T value, int expiresMinutes = 5);
        public void QueueUpdate(string key);
        public void QueueDelete(string key);
    }
}
