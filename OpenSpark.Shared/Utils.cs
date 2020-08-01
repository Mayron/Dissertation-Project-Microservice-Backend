using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSpark.Shared
{
    public static class Utils
    {
        // Removes the entity name portion of the RavenID to return just the raw
        // 11 character ID for use on the client side.
        public static string ConvertToClientId(this string ravenId)
        {
            var parts = ravenId.Split("/");
            return parts.Length == 2 ? parts[1] : parts[0];
        }

        public static string ConvertToRavenId<T>(this string clientId)
        {
            if (clientId.Contains("/")) throw new Exception($"Invalid client ID: {clientId}");
            return $"{typeof(T).Name.ToLower()}/{clientId}";
        }
        
        public static void RemoveAll<TK, TV>(this Dictionary<TK, TV> dictionary, Func<KeyValuePair<TK, TV>, bool> predicate)
        {
            foreach (var kv in dictionary.Where(predicate).ToList()) dictionary.Remove(kv.Key);
        }

        public static string ToTimeAgoFormat(this DateTime dateTime) => dateTime.ToString("yyyy:MM:dd:HH:mm:ss");
    }
}