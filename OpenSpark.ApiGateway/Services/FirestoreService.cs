using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using OpenSpark.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Services
{
    public interface IFirestoreService
    {
        Task<User> GetUserAsync(string authId);

        Task<User> GetUserAsync(string authId, CancellationToken cancellationToken);

        Task<bool> AddUserToGroupsAsync(User user, CancellationToken cancellationToken, params string[] groupIds);

        Task<bool> RemoveUserFromGroupAsync(User user, CancellationToken cancellationToken, string groupId);

        Task<bool> AddUserToProjectsAsync(User user, CancellationToken cancellationToken, params string[] projectIds);

        Task<bool> UpdateUserField(User user, string fieldName, object value, CancellationToken cancellationToken);
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

        public async Task<User> GetUserAsync(string authId) => await GetUserAsync(authId, CancellationToken.None);

        public async Task<User> GetUserAsync(string authId, CancellationToken cancellationToken)
        {
            try
            {
                var (_, snapShot) = await GetUserReferenceAsync(authId, cancellationToken);

                if (snapShot != null && snapShot.Exists)
                {
                    var user = new User
                    {
                        CreatedAt = snapShot.GetValue<Timestamp>("createdAt").ToDateTime(),
                        DisplayName = snapShot.GetValue<string>("displayName"),
                        LastOnline = snapShot.GetValue<Timestamp>("lastOnline").ToDateTime(),
                        IsOnline = snapShot.GetValue<bool>("isOnline"),
                        Email = snapShot.GetValue<string>("email"),
                        AuthUserId = authId,
                    };

                    if (snapShot.ContainsField("groups"))
                        user.Groups = snapShot.GetValue<List<string>>("groups");

                    if (snapShot.ContainsField("projects"))
                        user.Projects = snapShot.GetValue<List<string>>("projects");

                    return user;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception getting value from snapshot: {ex}");
            }

            return null;
        }

        public async Task<bool> AddUserToGroupsAsync(User user, CancellationToken cancellationToken, params string[] groupIds)
        {
            var groupIdsArray = groupIds.Select(g => (object)g).ToArray();
            return await UpdateUserField(user, "groups", FieldValue.ArrayUnion(groupIdsArray), cancellationToken);
        }

        public async Task<bool> RemoveUserFromGroupAsync(User user, CancellationToken cancellationToken, string groupId)
        {
            return await UpdateUserField(user, "groups", FieldValue.ArrayRemove(groupId), cancellationToken);
        }

        public async Task<bool> AddUserToProjectsAsync(User user, CancellationToken cancellationToken, params string[] projectIds)
        {
            var projectIdsArray = projectIds.Select(g => (object)g).ToArray();
            return await UpdateUserField(user, "projects", FieldValue.ArrayUnion(projectIdsArray), cancellationToken);
        }

        public async Task<bool> UpdateUserField(User user, string fieldName, object value, CancellationToken cancellationToken)
        {
            try
            {
                var (userRef, snapShot) = await GetUserReferenceAsync(user.AuthUserId, cancellationToken);

                if (snapShot != null && snapShot.Exists)
                {
                    if (value is DateTime dateTime)
                        value = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

                    await userRef.UpdateAsync(fieldName, value, cancellationToken: cancellationToken);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception updating user field '{fieldName}': {ex}");
            }

            return false;
        }

        private static async Task<(DocumentReference userRef, DocumentSnapshot snapShot)> GetUserReferenceAsync(
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