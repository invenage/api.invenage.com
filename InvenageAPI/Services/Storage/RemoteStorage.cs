using InvenageAPI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SortDirection = InvenageAPI.Services.Enum.SortDirection;

namespace InvenageAPI.Services.Storage
{
    public class RemoteStorage : IRemoteStorage
    {
        protected string connectionString;
        protected string storageType;
        protected IConfiguration _config;

        public RemoteStorage(IConfiguration config)
        {
            _config = config;
            storageType = "Remote";
            connectionString = config.GetConnectionString("RemoteStorage");
        }

        public string GetStorageType()
            => storageType;

        public bool Delete(string database, string collection, string id)
        {
            var result = ((MongoClient)GetDatabaseInstance()).GetDatabase(database).GetCollection<DataModel>(collection)
            .DeleteOne(x => x.Id == id);
            return result.IsAcknowledged;
        }

        public IEnumerable<T> Get<T>(QueryModel<T> query) where T : DataModel
        {
            var db = (MongoClient)GetDatabaseInstance();
            var filter = query.Filter;
            var order = query.Order;
            var sortDirection = query.SortDirection;
            var limit = query.Limit;
            var dbcollect = db.GetDatabase(query.Database).GetCollection<T>(query.Collection);
            return (filter, order, sortDirection) switch
            {
                (null, null, SortDirection.Asc or SortDirection.Desc) => dbcollect.Find(_ => true).Limit(limit).ToList(),
                (not null, null, SortDirection.Asc or SortDirection.Desc) => dbcollect.Find(filter).Limit(limit).ToList(),
                (null, not null, SortDirection.Asc) => dbcollect.Find(_ => true).SortBy(order).Limit(limit).ToList(),
                (null, not null, SortDirection.Desc) => dbcollect.Find(_ => true).SortByDescending(order).Limit(limit).ToList(),
                (not null, not null, SortDirection.Asc) => dbcollect.Find(filter).SortBy(order).Limit(limit).ToList(),
                (not null, not null, SortDirection.Desc) => dbcollect.Find(filter).SortByDescending(order).Limit(limit).ToList(),
                _ => throw new NotSupportedException()
            };
        }

        public bool Save<T>(string database, string collection, T data, bool isSync) where T : DataModel
        {
            ((MongoClient)GetDatabaseInstance()).GetDatabase(database).GetCollection<T>(collection)
            .ReplaceOne(x => x.Id == data.Id, data, new ReplaceOptions() { IsUpsert = true });
            return true;
        }

        public object GetDatabaseInstance(string database = null)
            => new MongoClient(connectionString);

        public async Task<IEnumerable<T>> GetAsync<T>(QueryModel<T> query) where T : DataModel
        {
            var db = (MongoClient)GetDatabaseInstance();
            var filter = query.Filter;
            var order = query.Order;
            var sortDirection = query.SortDirection;
            var limit = query.Limit;
            var dbcollect = db.GetDatabase(query.Database).GetCollection<T>(query.Collection);

            return await ((filter, order, sortDirection) switch
            {
                (null, null, SortDirection.Asc or SortDirection.Desc) => dbcollect.Find(_ => true).Limit(limit).ToListAsync(),
                (not null, null, SortDirection.Asc or SortDirection.Desc) => dbcollect.Find(filter).Limit(limit).ToListAsync(),
                (null, not null, SortDirection.Asc) => dbcollect.Find(_ => true).SortBy(order).Limit(limit).ToListAsync(),
                (null, not null, SortDirection.Desc) => dbcollect.Find(_ => true).SortByDescending(order).Limit(limit).ToListAsync(),
                (not null, not null, SortDirection.Asc) => dbcollect.Find(filter).SortBy(order).Limit(limit).ToListAsync(),
                (not null, not null, SortDirection.Desc) => dbcollect.Find(filter).SortByDescending(order).Limit(limit).ToListAsync(),
                _ => throw new NotSupportedException()
            });
        }

        public async Task<bool> SaveAsync<T>(string database, string collection, T data) where T : DataModel
        {
            await ((MongoClient)GetDatabaseInstance()).GetDatabase(database).GetCollection<T>(collection)
                .ReplaceOneAsync(x => x.Id == data.Id, data, new ReplaceOptions() { IsUpsert = true });
            return true;
        }

        public async Task<bool> DelectAsync(string database, string collection, string id)
        {
            var result = await ((MongoClient)GetDatabaseInstance()).GetDatabase(database).GetCollection<DataModel>(collection)
            .DeleteOneAsync(x => x.Id == id);
            return result.IsAcknowledged;
        }
    }
}
