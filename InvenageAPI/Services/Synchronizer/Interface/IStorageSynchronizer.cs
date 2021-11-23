using InvenageAPI.Models;

namespace InvenageAPI.Services.Synchronizer
{
    public interface IStorageSynchronizer : ISynchronizer
    {
        public void Queue<T>(string database, string collection, T data) where T : DataModel;
        public void QueueDelete(string database, string collection, string id);
    }
}
