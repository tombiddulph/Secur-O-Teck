using System;
using System.Collections.Generic;
using System.Text;

namespace SecuroteckClient
{
    public static class Extensions
    {
        public static string ReplaceAll(this string intput, params string[] toRemove)
        {
            var sb = new StringBuilder(intput);
            toRemove.ForEach(x => sb.Replace(x, string.Empty));
            return sb.ToString();
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }
    }
}
