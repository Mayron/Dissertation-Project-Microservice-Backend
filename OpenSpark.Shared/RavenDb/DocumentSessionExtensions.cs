using System.Linq;
using OpenSpark.Domain;
using Raven.Client.Documents.Session;

namespace OpenSpark.Shared.RavenDb
{
    public static class DocumentSessionExtensions
    {
        public static string GenerateRavenId<T>(this IDocumentSession session) where T : IEntity
        {
            while (true)
            {
                var newId = Utils.GenerateRandomId(typeof(T).Name.ToLower());
                var existingEntity = session.Query<T>().FirstOrDefault(g => g.Id == newId);

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
    }
}