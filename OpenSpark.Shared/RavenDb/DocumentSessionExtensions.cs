using System;
using System.Linq;
using System.Text;
using OpenSpark.Domain;
using Raven.Client.Documents.Session;
using Slugify;

namespace OpenSpark.Shared.RavenDb
{
    public static class DocumentSessionExtensions
    {
        private static readonly char[] Characters = new string(
                "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_")
            .ToCharArray();

        public static string GenerateRavenId<T>(this IDocumentSession session) where T : IEntity
        {
            while (true)
            {
                var newId = GenerateRandomId(typeof(T).Name.ToLower());
                var existingEntity = session.Load<T>(newId);

                if (existingEntity == null) return newId;
            }
        }

        public static string GenerateRavenIdFromName<T>(this IDocumentSession session, string name) where T : INamedEntity
        {
            var slugHelper = new SlugHelper();

            while (true)
            {
                var slug = slugHelper.GenerateSlug(name);
                var newId = $"{typeof(T).Name.ToLower()}/{slug}";
                var existingEntity = session.Load<T>(newId);

                if (existingEntity == null) return newId;
            }
        }

        public static bool IsNameTaken<T>(this IDocumentSession session, string name) where T : INamedEntity
        {
            // Note: RavenDB is case-insensitive while comparing strings
            // https://ravendb.net/docs/article-page/3.0/Csharp/indexes/using-analyzers
            var alreadyExists = session.Query<T>().FirstOrDefault(g => g.Name == name);
            return alreadyExists != null;
        }

        private static string GenerateRandomId(string className)
        {
            var id = new StringBuilder((className.Length + 1) + 11);
            var random = new Random();

            id.AppendFormat("{0}/", className.ToLower());
            for (var i = 0; i < 11; i++)
            {
                var randomCharacter = Characters[random.Next(Characters.Length)];
                id.Append(randomCharacter);
            }

            return id.ToString();
        }
    }
}