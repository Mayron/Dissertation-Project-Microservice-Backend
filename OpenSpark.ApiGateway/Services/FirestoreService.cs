using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using OpenSpark.Domain;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Services
{
    public interface IFirestoreService
    {
        Task<User> GetUserAsync(string authId, CancellationToken cancellationToken);

        Task<bool> AddUserToGroupsAsync(User user, params Group[] groups);

        Task<bool> RemoveUserFromGroupAsync(User user, string groupId);
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
                var (_, snapShot) = await GetUserReference(authId, cancellationToken);

                if (snapShot != null && snapShot.Exists)
                {
                    return new User
                    {
                        CreatedAt = snapShot.GetValue<Timestamp>("createdAt").ToDateTime(),
                        DisplayName = snapShot.GetValue<string>("displayName"),
                        Email = snapShot.GetValue<string>("email"),
                        AuthUserId = authId,
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting value from snapshot: {ex}");
            }

            return null;
        }

        public async Task<bool> AddUserToGroupsAsync(User user, params Group[] groups)
        {
            try
            {
                var (userRef, snapShot) = await GetUserReference(user.AuthUserId, CancellationToken.None);

                if (snapShot != null && snapShot.Exists)
                {
                    var groupIds = groups.Select(g => (object)g.GroupId).ToArray();
                    await userRef.UpdateAsync("groups", FieldValue.ArrayUnion(groupIds));

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception updating user groups: {ex}");
            }

            return false;
        }

        public async Task<bool> RemoveUserFromGroupAsync(User user, string groupId)
        {
            try
            {
                var (userRef, snapShot) = await GetUserReference(user.AuthUserId, CancellationToken.None);

                if (snapShot != null && snapShot.Exists)
                {
                    await userRef.UpdateAsync("groups", FieldValue.ArrayRemove(groupId));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception updating user groups: {ex}");
            }

            return false;
        }

        private static async Task<(DocumentReference userRef, DocumentSnapshot snapShot)> GetUserReference(
            string authId, CancellationToken cancellationToken)
        {
            try
            {
                var collection = Store.Collection("users");
                var userRef = collection.Document(authId);
                var snapShot = await userRef.GetSnapshotAsync(cancellationToken);

                return (userRef, snapShot);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting user: {ex}");
            }

            return (null, null);
        }
    }
}