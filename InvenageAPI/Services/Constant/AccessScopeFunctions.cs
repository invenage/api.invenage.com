using InvenageAPI.Services.Extension;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InvenageAPI.Services.Constant
{
    public static partial class AccessScope
    {
        public static bool IsVaildScope(string input)
            => GetScopesList().Any(x => x == input);

        public static bool IsVaildScope(List<string> input)
            => GetScopesList().Intersect(input).Count() == input.Count;

        public static IEnumerable<string> GetScopesList()
            => typeof(AccessScope)
               .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
               .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
               .Select(x => (string)x.GetRawConstantValue())
               .Where(x => !x.IsNullOrEmpty());
    }
}
