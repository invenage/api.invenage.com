using InvenageAPI.Services.Global;
using System;

namespace InvenageAPI.Models
{
    public abstract class DataModel
    {
        [LiteDB.BsonId]
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        public string Id { internal get; set; }
        public long RecordTime { internal get; set; }

        public DataModel()
        {
            Id = Guid.NewGuid().ToString();
            RecordTime = GlobalVariable.CurrentTime;
        }
    }
}
