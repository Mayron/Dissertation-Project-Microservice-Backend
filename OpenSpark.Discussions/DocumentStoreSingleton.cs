using OpenSpark.Discussions.Indexes;
using OpenSpark.Shared;
using Raven.Client.Documents;
using System;
using System.Reflection;

namespace OpenSpark.Discussions
{
    public static class DocumentStoreSingleton
    {
        private static readonly Lazy<IDocumentStore> _store = new Lazy<IDocumentStore>(CreateDocumentStore);

        public static IDocumentStore Store => _store.Value;

        private static IDocumentStore CreateDocumentStore()
        {
            if (_store.IsValueCreated) return _store.Value;

            const string databaseName = "OpenSpark.Discussions";
            const string url = "http://127.0.0.1:8080/";
            var indexAssembly = Assembly.GetAssembly(typeof(GetGroupPosts));

            return RavenDbDocumentStoreFactory.CreateDocumentStore(databaseName, url, indexAssembly);
        }
    }
}