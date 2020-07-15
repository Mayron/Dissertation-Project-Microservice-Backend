using System;
using System.Text;

namespace OpenSpark.Shared
{
    public static class Utils
    {
        private static readonly char[] Characters = new string("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_").ToCharArray();

        public static string GenerateRandomId()
        {
            var groupId = new StringBuilder(11);
            var random = new Random();

            for (var i = 0; i < 11; i++)
            {
                var randomCharacter = Characters[random.Next(Characters.Length)];
                groupId.Append(randomCharacter);
            }

            return groupId.ToString();
        }
    }
}