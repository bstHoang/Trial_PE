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

        private static string Stringify(object item, HashSet<object> visited, int indent = 0)
        {
            if (item == null) return "null";

            var type = item.GetType();

            bool isReference = !(type.IsValueType || item is string);
            if (isReference)
            {
                if (visited.Contains(item))
                    return "(circular reference)";
                visited.Add(item);
            }

            string indentStr = new string(' ', indent * 2);
            string nextIndent = new string(' ', (indent + 1) * 2);

            // ✅ Handle primitive & simple types
            if (type.IsPrimitive || item is decimal || item is bool) return item.ToString();
            if (item is string str) return str;

            // ✅ Handle collections (don’t reflect their internal props)
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                var listLines = new List<string>();
                foreach (var element in (System.Collections.IEnumerable)item)
                {
                    var inner = Stringify(element, visited, indent + 1);
                    if (IsComplex(element))
                        listLines.Add($"\n{nextIndent}- {inner}");
                    else
                        listLines.Add($"- {inner}");
                }
                return string.Join(", ", listLines);
            }

            // ✅ Handle dictionary
            if (typeof(System.Collections.IDictionary).IsAssignableFrom(type))
            {
                var dict = (System.Collections.IDictionary)item;
                var entries = new List<string>();
                foreach (var key in dict.Keys)
                {
                    var value = dict[key];
                    var jsonVal = Stringify(value, visited, indent + 1);
                    entries.Add($"{key}: {jsonVal}");
                }
                return string.Join(", ", entries);
            }

            // ✅ Handle custom objects
            var props = type.GetProperties();
            var propLines = new List<string>();

            foreach (var prop in props)
            {
                // skip indexer properties
                if (prop.GetIndexParameters().Length > 0) continue;

                var value = prop.GetValue(item);
                var valStr = Stringify(value, visited, indent + 1);

                if (IsComplex(value))
                    propLines.Add($"\n{nextIndent}{prop.Name}: {valStr}");
                else
                    propLines.Add($"{prop.Name}: {valStr}");
            }

            bool hasInner = props.Any(p => IsComplex(p.GetValue(item)));
            return hasInner
                ? string.Join(",", propLines) + $"\n{indentStr}"
                : string.Join(", ", propLines);
        }

        private static bool IsComplex(object obj)
        {
            if (obj == null) return false;
            var t = obj.GetType();
            return !(t.IsPrimitive || obj is string || obj is decimal || obj is bool);
        }
    }
}
