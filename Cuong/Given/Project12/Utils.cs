using System.Reflection;
using System.Text;
using Project12.Models;

namespace Project12
{
    public class Utils
    {
        // We are modifying this method to fix the duplicate output.
        public static void FormatObject<T>(T item)
        {
            // If the item is null, we do nothing.
            if (item == null)
            {
                Console.WriteLine(string.Empty);
                return; // Exit the method early.
            }

            // Check if the item is a Movie object.
            if (item is Movie movie)
            {
                // This block prints the success message and the formatted movie details,
                // just like in your example image.
                Console.WriteLine(
                    $"Successfully borrowed '{movie.Title}'. Remaining copies: {movie.AvailableCopies}");
                Console.WriteLine($"Title: {movie.Title}, ReleaseYear: {movie.ReleaseYear}, Genre: {movie.Genre}, AvailableCopies: {movie.AvailableCopies}, Director: {movie.Director?.FirstName} {movie.Director?.LastName}");

                // We add a 'return' here to prevent any other code in this method from running.
                // This is the key to solving the duplication issue.
                return;
            }

            // If the object is not a Movie, you could add other formatting rules here
            // or use the Stringify method for a generic output. For now, we'll leave it simple.
            Console.WriteLine(Stringify(item));
        }

        // The Stringify methods remain unchanged.
        public static string Stringify<T>(T item)
        {
            return Stringify(item!, new HashSet<object>());
        }

        private static string Stringify(object item, HashSet<object> visited)
        {
            if (item == null || visited.Contains(item)) return string.Empty;
            visited.Add(item);

            var type = item.GetType();

            // Handle Dictionary
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

            // Handle List, Set, etc.
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                var listBuilder = new StringBuilder();
                foreach (var element in (System.Collections.IEnumerable)item)
                {
                    listBuilder.Append(Stringify(element, visited));
                }
                return listBuilder.ToString();
            }

            // Handle regular object
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