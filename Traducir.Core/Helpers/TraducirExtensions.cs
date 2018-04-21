using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Traducir.Core.Helpers
{
    public static class TraducirExtensions
    {
        public static HashSet<T> ToHashSet<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool HasValue(this string str)
        {
            return !str.IsNullOrEmpty();
        }

        #pragma warning disable CA5351
        public static string CalculateMd5(this string str)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(str);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2", CultureInfo.InvariantCulture));
                }

                return sb.ToString();
            }
        }
        #pragma warning restore CA5351

        public static T GetClaim<T>(this ClaimsPrincipal user, string type)
        {
            var value = user.Claims.FirstOrDefault(c => c.Type == type)?.Value;
            if (value == null)
            {
                return default(T);
            }

            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromString(value);
        }

        public static string ToNormalizedKey(this string key)
        {
            if (!key.Contains("|"))
            {
                return key;
            }

            var parts = key.Split('|');
            var variables = parts[1].Split(',').OrderBy(v => v);

            return $"{parts[0]}|{string.Join(",", variables)}";
        }
    }
}