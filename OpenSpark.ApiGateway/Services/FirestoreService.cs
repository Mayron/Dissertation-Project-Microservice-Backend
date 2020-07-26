using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using OpenSpark.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.Shared;

namespace OpenSpark.ApiGateway.Services
{
    public interface IFirestoreService
    {
        Task<User> GetUserAsync(string authId);
        Task<User> GetUserAsync(string authId, CancellationToken cancellationToken);
        Task<bool> AddUserToGroupsAsync(User user, params Group[] groups);
        Task<bool> RemoveUserFromGroupAsync(User user, string groupId);
        Task<bool> AddUserToProjectsAsync(User user, params Project[] projects);
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
                var (_, snapShot) = await GetUserReference(authId, cancellationToken);

                if (snapShot != null && snapShot.Exists)
                {
                    var user = new User
                    {
                        CreatedAt = snapShot.GetValue<Timestamp>("createdAt").ToDateTime(),
                        DisplayName = snapShot.GetValue<string>("displayName"),
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

        public async Task<bool> AddUserToGroupsAsync(User user, params Group[] groups)
        {
            var groupIds = groups.Select(g => (object)g.Id.ConvertToEntityId()).ToArray();
            return await UpdateUserArrayField(user, "groups", groupIds);
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

        public async Task<bool> AddUserToProjectsAsync(User user, params Project[] projects)
        {
            var projectIds = projects.Select(g => (object)g.Id.ConvertToEntityId()).ToArray();
            return await UpdateUserArrayField(user, "projects", projectIds);
        }

        private static async Task<bool> UpdateUserArrayField(User user, string fieldName, object[] values)
        {
            try
            {
                var (userRef, snapShot) = await GetUserReference(user.AuthUserId, CancellationToken.None);

                if (snapShot != null && snapShot.Exists)
                {
                    await userRef.UpdateAsync(fieldName, FieldValue.ArrayUnion(values));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception updating user {fieldName}: {ex}");
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