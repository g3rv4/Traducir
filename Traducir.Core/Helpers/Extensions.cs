using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Traducir.Core.Helpers
{
    public static class Extensions
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

        public static string CalculateMd5(this string str)
        {
            // Use input string to calculate MD5 hash
            using(System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(str);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static T GetClaim<T>(this ClaimsPrincipal user, string type)
        {
            var value = user.Claims.FirstOrDefault(c => c.Type == type)? .Value;
            if (value == null)
            {
                return default(T);
            }

            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromString(value);
        }
    }
}