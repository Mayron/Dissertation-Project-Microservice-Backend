using System;
using OpenSpark.Shared;
using Raven.Client.Documents;

namespace OpenSpark.Groups
{
    public static class DocumentStoreSingleton
    {
        private static readonly Lazy<IDocumentStore> _store = new Lazy<IDocumentStore>(CreateDocumentStore);

        public static IDocumentStore Store => _store.Value;

        private static IDocumentStore CreateDocumentStore()
        {
            if (_store.IsValueCreated) return _store.Value;

            const string databaseName = "OpenSpark.Groups";
            const string url = "http://127.0.0.1:8080/";

            return RavenDbDocumentStoreFactory.CreateDocumentStore(databaseName, url);
        }
    }
}