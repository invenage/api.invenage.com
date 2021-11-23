using System;
using System.Collections.Generic;

namespace InvenageAPI.Services.Extension
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
                action(item);
        }

        public static IEnumerable<TResult> ForEach<T, TResult>(this IEnumerable<T> list, Func<T, TResult> func)
        {
            var result = new List<TResult>();
            foreach (var item in list)
                result.Add(func(item));
            return result;
        }
    }
}