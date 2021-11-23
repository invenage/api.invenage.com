using InvenageAPI.Models;
using LiteDB;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvenageAPI.Services.Storage
{
    public interface IStorage
    {
        public string GetStorageType();
        public IEnumerable<T> Get<T>(QueryModel<T> query) where T : DataModel;
        public bool Save<T>(string database, string collection, T data, bool isSync = true) where T : DataModel;
        public bool Delete(string database, string collection, string id);
        public Task<IEnumerable<T>> GetAsync<T>(QueryModel<T> query) where T : DataModel;
        public Task<bool> SaveAsync<T>(string database, string collection, T data) where T : DataModel;
        public Task<bool> DelectAsync(string database, string collection, string id);

        /// <summary>
        /// Get the Database instance. <see cref="LiteDatabase"/> if local and <see cref="MongoClient"/> if remote/achieve. Remember to disconnect or dispose after use.
        /// </summary>
        /// <param name="database">The database file for local, ignore if remote</param>
        /// <returns>
        /// <see cref="LiteDatabase"/> if local, <see cref="MongoClient"/> if remote/achieve
        /// </returns>
        public object GetDatabaseInstance(string database);
    }
}
