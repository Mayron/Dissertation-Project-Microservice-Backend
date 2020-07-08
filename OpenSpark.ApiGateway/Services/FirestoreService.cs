using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.Extensions.Configuration;
using OpenSpark.ApiGateway.Services.SDK;
using OpenSpark.Domain;

namespace OpenSpark.ApiGateway.Services
{
    public class FirestoreService : IFirestoreService
    {
        private static readonly Lazy<FirestoreDb> _store = new Lazy<FirestoreDb>(
            () => FirestoreDb.Create("openspark-1e4bc"));

        public static FirestoreDb Store => _store.Value;

        public FirestoreService(IConfiguration configuration)
        {
            var credentialsEnvName = configuration["firestore:CredentialsEnvName"];
            var path = AppDomain.CurrentDomain.BaseDirectory + @"openspark-1e4bc-firebase-adminsdk-as86c-e46fdfc058.json";
            Environment.SetEnvironmentVariable(credentialsEnvName, path);

        }

        public async Task<User> GetUserAsync(string authId, CancellationToken cancellationToken)
        {
            User user = null;

            try
            {
                var collection = Store.Collection("users");
                var userRef = collection.Document(authId);
                var snapShot = await userRef.GetSnapshotAsync(cancellationToken);

                if (snapShot.Exists)
                {
                    var userRaw = snapShot.ToDictionary();
                    // TODO: Convert manually!
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception getting user: ", ex);
            }

            return user;
        }
    }
}
