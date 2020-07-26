using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSpark.Shared
{
    public static class Utils
    {
        // Removes the entity name portion of the RavenID to return just the raw
        // 11 character ID for use on the client side.
        public static string ConvertToEntityId(this string ravenId) => ravenId.Split("/")[1];

        public static string ConvertToRavenId<T>(this string entityId) => $"{typeof(T).Name.ToLower()}/{entityId}";

        public static void RemoveAll<TK, TV>(this Dictionary<TK, TV> dictionary, Func<KeyValuePair<TK, TV>, bool> predicate)
        {
            foreach (var kv in dictionary.Where(predicate).ToList()) dictionary.Remove(kv.Key);
        }

        public static string ToTimeAgoFormat(this DateTime dateTime) => dateTime.ToString("yyyy:MM:dd:HH:mm:ss");
    }
}