using InvenageAPI.Models;
using InvenageAPI.Services.Enum;
using InvenageAPI.Services.Extension;
using InvenageAPI.Services.Synchronizer;
using LiteDB;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InvenageAPI.Services.Storage
{
    public class LocalStorage : ILocalStorage
    {
        protected string connectionString;
        protected string storageType;
        private readonly IStorageSynchronizer _storageSynchronizer;
        private readonly IStorage _remoteStorage;
        private readonly ConcurrentDictionary<string, LiteDatabase> databases = new();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<bool>> locks = new();

        public LocalStorage(IConfiguration config, IRemoteStorage remoteStorage, IStorageSynchronizer storageSynchronizer)
        {
            storageType = "Local";
            connectionString = config.GetConnectionString("LocalStorage");
            _remoteStorage = remoteStorage;
            _storageSynchronizer = storageSynchronizer;
        }

        public string GetStorageType()
            => storageType;

        public bool Delete(string database, string collection, string id)
        {
            var db = (LiteDatabase)GetDatabaseInstance(database);
            var success = db.GetCollection<DataModel>(collection).Delete(id);
            if (success)
                _storageSynchronizer.QueueDelete(database, collection, id);
            RemoveLocks(database);
            return success;
        }

        public IEnumerable<T> Get<T>(QueryModel<T> query) where T : DataModel
        {
            var db = (LiteDatabase)GetDatabaseInstance(query.Database);
            var filter = query.Filter;
            var order = query.Order;
            var sortDirection = query.SortDirection;
            var limit = query.Limit;
            var dbcollect = db.GetCollection<T>(query.Collection);
            IEnumerable<T> result = (filter, order, sortDirection) switch
            {
                (null, null, SortDirection.Asc or SortDirection.Desc) => dbcollect.Find(x => true, 0, limit),
                (not null, null, SortDirection.Asc or SortDirection.Desc) => dbcollect.Find(filter, 0, limit),
                (null, not null, SortDirection.Asc) => dbcollect.FindAll().OrderBy(order.Compile()).Take(limit),
                (null, not null, SortDirection.Desc) => dbcollect.FindAll().OrderByDescending(order.Compile()).Take(limit),
                (not null, not null, SortDirection.Asc) => dbcollect.Find(filter, 0, limit).OrderBy(order.Compile()).Take(limit),
                (not null, not null, SortDirection.Desc) => dbcollect.Find(filter, 0, limit).OrderByDescending(order.Compile()).Take(limit),
                _ => throw new NotSupportedException()
            };
            result = result.ToList();
            if (!result.Any())
            {
                result = _remoteStorage.Get(query);
                if (result.Any())
                    result.ForEach(async x => await Task.Run(() => Save(query.Database, query.Collection, x, false)));
            }
            RemoveLocks(query.Database);
            return result;
        }

        public bool Save<T>(string database, string collection, T data, bool isSync = true) where T : DataModel
        {
            var db = (LiteDatabase)GetDatabaseInstance(database);
            if (db.GetCollection<T>(collection).Upsert(data) && isSync)
                _storageSynchronizer.Queue(database, collection, data);
            RemoveLocks(database);
            return true;
        }

        public object GetDatabaseInstance(string database)
        {
            if (!locks.ContainsKey(database))
                locks.TryAdd(database, new ConcurrentQueue<bool>());

            if (!databases.TryGetValue(database, out LiteDatabase db))
            {
                db = new LiteDatabase(Path.Combine(connectionString, database) + ".db");
                databases.TryAdd(database, db);
            }
            AddLocks(database);
            return db;
        }

        private void AddLocks(string database)
         => locks[database].Enqueue(true);

        private void RemoveLocks(string database)
        {
            locks[database].TryDequeue(out _);
            if (locks[database].IsEmpty && databases.TryGetValue(database, out LiteDatabase db))
            {
                db.Dispose();
                databases.Remove(database, out _);
            }
        }

        public async Task<IEnumerable<T>> GetAsync<T>(QueryModel<T> query) where T : DataModel
            => await Task.Run(() => Get(query));

        public async Task<bool> SaveAsync<T>(string database, string collection, T data) where T : DataModel
            => await Task.Run(() => Save(database, collection, data));

        public async Task<bool> DelectAsync(string database, string collection, string id)
            => await Task.Run(() => Delete(database, collection, id));

    }
}
