using System;
using System.Linq;
using System.Reflection;
using OpenSpark.Groups.Domain;
using OpenSpark.Groups.Indexes;
using OpenSpark.Shared;
using OpenSpark.Shared.RavenDb;
using Raven.Client.Documents;

namespace OpenSpark.Groups
{
    public static class DocumentStoreSingleton
    {
        private static readonly Lazy<IDocumentStore> LazyStore = new Lazy<IDocumentStore>(CreateDocumentStore);

        public static IDocumentStore Store => LazyStore.Value;

        private static IDocumentStore CreateDocumentStore()
        {
            if (LazyStore.IsValueCreated) return LazyStore.Value;

            const string databaseName = "OpenSpark.Groups";
            const string url = "http://127.0.0.1:8080/";
            var indexAssembly = Assembly.GetAssembly(typeof(GetBasicGroupDetails));

            var store = RavenDbDocumentStoreFactory.CreateDocumentStore(databaseName, url, indexAssembly);

            if (store != null) SeedData(store);

            return store;
        }

        private static void SeedData(IDocumentStore store)
        {
            using var session = store.OpenSession();

            if (!session.Query<Category>().Any())
            {
                var categories = new []
                {
                    "Gaming", "Education", "Entertainment", "News and Politics", 
                    "Science and Technology", "Travel and Events", "Pets and animals",
                    "Music", "Arts and Crafts", "Film and animation", "Digital Media", 
                    "Non-profit / Activism", "Internet Personality", "Business"
                };

                foreach (var category in categories)
                {
                    session.Store(new Category
                    {
                        Id = session.GenerateRavenId<Category>(),
                        Name = category
                    });
                }
            }

            session.SaveChanges();
        }
    }
}