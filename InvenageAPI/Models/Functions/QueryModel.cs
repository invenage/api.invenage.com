using InvenageAPI.Services.Enum;
using System;
using System.Linq.Expressions;

namespace InvenageAPI.Models
{
    public class QueryModel<T>
    {
        public string Database = "default";
        public string Collection = "default";
        public Expression<Func<T, bool>> Filter = null;
        public Expression<Func<T, object>> Order = null;
        public SortDirection SortDirection = SortDirection.Asc;
        public int Limit = 100;
    }
}
