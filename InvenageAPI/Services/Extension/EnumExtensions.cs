using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvenageAPI.Services.Extension
{
    public static class EnumExtensions
    {
        public static T PraseAsEnum<T>(this string value) where T : System.Enum
            => (T)System.Enum.Parse(typeof(T), value);
    }
}
