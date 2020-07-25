using System;

namespace OpenSpark.Shared
{
    public static class Utils
    {
        // Removes the entity name portion of the RavenID to return just the raw
        // 11 character ID for use on the client side.
        public static string ConvertToEntityId(this string ravenId) => ravenId.Split("/")[1];

        public static string ConvertToRavenId<T>(this string entityId) => $"{typeof(T).Name.ToLower()}/{entityId}";
    }
}