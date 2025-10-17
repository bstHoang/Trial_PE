using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server
{
    internal class Helper
    {
        private Helper() { }

        /// <summary>
        /// Student must use this method to format the object before printing it to the console
        /// </summary>
        /// <returns>string contain all item attribute</returns>
        public static string Stringify<T>(T item)
        {
            return Stringify(item!, new HashSet<object>());
        }

        private static string Stringify(object item, HashSet<object> visited)
        {
            if (item == null || visited.Contains(item)) return string.Empty;
            visited.Add(item);

            var type = item.GetType();

            if (typeof(System.Collections.IDictionary).IsAssignableFrom(type))
            {
                var dict = (System.Collections.IDictionary)item;
                var builderDict = new StringBuilder();
                foreach (var key in dict.Keys)
                {
                    var value = dict[key];
                    builderDict.Append($"[{Stringify(key, visited)}] = {Stringify(value, visited)} | ");
                }
                return builderDict.ToString();
            }

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                var listBuilder = new StringBuilder();
                foreach (var element in (System.Collections.IEnumerable)item)
                {
                    listBuilder.Append(Stringify(element, visited));
                }
                return listBuilder.ToString();
            }

            var builder = new StringBuilder();
            foreach (var prop in type.GetProperties())
            {
                var value = prop.GetValue(item);
                if (value == null)
                {
                    builder.Append($"{prop.Name}: null | ");
                    continue;
                }

                if (IsCustomObject(prop.PropertyType))
                {
                    builder.Append($"\n{prop.Name}:\n\t {Stringify(value, visited)} ");
                }
                else
                {
                    builder.Append($"{prop.Name}: {value} | ");
                }
            }
            return builder.ToString();
        }

        private static bool IsCustomObject(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return !(type.IsPrimitive || type.IsEnum || type == typeof(string) || type.IsValueType);
        }
    }
}
