using System;
using System.Text;

namespace OpenSpark.Shared
{
    public static class Utils
    {
        private static readonly char[] Characters = new string(
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_")
            .ToCharArray();

        public static string GenerateRandomId(string entityName)
        {
            var id = new StringBuilder((entityName.Length + 1) + 11);
            var random = new Random();

            id.AppendFormat("{0}/", entityName.ToLower());
            for (var i = 0; i < 11; i++)
            {
                var randomCharacter = Characters[random.Next(Characters.Length)];
                id.Append(randomCharacter);
            }

            return id.ToString();
        }

        // Removes the entity name portion of the RavenID to return just the raw
        // 11 character ID for use on the client side.
        public static string GetUrlFriendlyId(string ravenId) => ravenId.Split("/")[1];
    }
}