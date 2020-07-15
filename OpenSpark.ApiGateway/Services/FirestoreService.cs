using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.Domain;

namespace OpenSpark.ApiGateway.Services
{
    public interface IFirestoreService
    {
        Task<User> GetUserAsync(string authId, CancellationToken cancellationToken);
        Task<User> GetUserAsync(ClaimsPrincipal user, CancellationToken cancellationToken);
    }

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
            try
            {
                var collection = Store.Collection("users");
                var userRef = collection.Document(authId);
                var snapShot = await userRef.GetSnapshotAsync(cancellationToken);

                if (snapShot.Exists)
                {
                    return new User
                    {
                        CreatedAt = snapShot.GetValue<Timestamp>("createdAt").ToDateTime(),
                        DisplayName = snapShot.GetValue<string>("displayName"),
                        Email = snapShot.GetValue<string>("email"),
                        UserId = authId,
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception getting user: ", ex);
            }

            return null;
        }

        public async Task<User> GetUserAsync(ClaimsPrincipal userPrincipal, CancellationToken cancellationToken)
        {
            var authId = userPrincipal.GetFirebaseAuth();

            if (string.IsNullOrWhiteSpace(authId)) return null;

            return await GetUserAsync(authId, cancellationToken);
        }
    }
}
