using OpenSpark.Discussions.Indexes;
using OpenSpark.Shared;
using Raven.Client.Documents;
using System;
using System.Reflection;
using OpenSpark.Shared.RavenDb;

namespace OpenSpark.Discussions
{
    public static class DocumentStoreSingleton
    {
        private static readonly Lazy<IDocumentStore> LazyStore = new Lazy<IDocumentStore>(CreateDocumentStore);

        public static IDocumentStore Store => LazyStore.Value;

        private static IDocumentStore CreateDocumentStore()
        {
            if (LazyStore.IsValueCreated) return LazyStore.Value;

            const string databaseName = "OpenSpark.Discussions";
            const string url = "http://127.0.0.1:8080/";
            var indexAssembly = Assembly.GetAssembly(typeof(GetGroupPosts));

            return RavenDbDocumentStoreFactory.CreateDocumentStore(databaseName, url, indexAssembly);
        }
    }
}