using System;
using System.Reflection;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace OpenSpark.Shared.RavenDb
{
    public static class RavenDbDocumentStoreFactory
    {
        public static IDocumentStore CreateDocumentStore(string databaseName, string url, Assembly indexAssembly = null)
        {
            var store = new DocumentStore
            {
                Urls = new[] { url },
                Conventions =
                {
                    MaxNumberOfRequestsPerSession = 1024,
                    UseOptimisticConcurrency = true
                },
                Database = databaseName,
                // Define a client certificate (optional)
                // Certificate = new X509Certificate2("C:\\path_to_your_pfx_file\\cert.pfx"),
            };

            try
            {
                // Initialize the Document Store
                store.Initialize();

                try
                {
                    // Attempt to create the database if it does not already exist
                    store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(store.Database)));
                }
                catch (Exception)
                {
                    // database exists
                }

                if (indexAssembly != null)
                {
                    IndexCreation.CreateIndexes(indexAssembly, store);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize RavenDB: {ex}");
            }

            return store;
        }
    }
}
