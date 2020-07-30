using OpenSpark.Shared.RavenDb;
using Raven.Client.Documents;
using System;

namespace OpenSpark.Teams
{
    public static class DocumentStoreSingleton
    {
        private static readonly Lazy<IDocumentStore> _store = new Lazy<IDocumentStore>(CreateDocumentStore);

        public static IDocumentStore Store => _store.Value;

        private static IDocumentStore CreateDocumentStore()
        {
            if (_store.IsValueCreated) return _store.Value;

            const string databaseName = "OpenSpark.Teams";
            const string url = "http://127.0.0.1:8080/";

            return RavenDbDocumentStoreFactory.CreateDocumentStore(databaseName, url);
        }
    }
}