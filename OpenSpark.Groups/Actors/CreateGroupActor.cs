using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Events.Sagas;
using OpenSpark.Shared.Events.Sagas.CreateGroup;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSpark.Groups.Actors
{
    public class CreateGroupActor : ReceiveActor
    {
        public CreateGroupActor()
        {
            Receive<CreateGroupCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                var isValid = IsNewGroupValid(command, session);
                if (!isValid)
                {
                    Sender.Tell(new SagaErrorEvent
                    {
                        Message = "Group name taken",
                        TransactionId = command.TransactionId
                    });

                    Self.GracefulStop(TimeSpan.FromSeconds(5));
                    return;
                }

                var newGroupId = GenerateGroupIdAsync(session);
                var group = new Group
                {
                    Id = newGroupId,
                    OwnerUserId = command.User.AuthUserId,
                    Members = new List<string> { command.User.AuthUserId },
                    About = command.About,
                    Name = command.Name,
                    CategoryId = command.CategoryId,
                    Tags = command.Tags,
                    Visibility = VisibilityStatus.Public, // TODO: Needs to be configurable on creation
                    ConnectedProjects = command.Connected,
                    Roles = new List<Role>(),
                    BannedUsers = new List<string>(),
                    CreatedAt = DateTime.Now,
                };

                var member = new Member
                {
                    GroupId = group.Id,
                    Id = Utils.GenerateRandomId("member"),
                    AuthUserId = command.User.AuthUserId,
                    Joined = DateTime.Now,
                };

                session.Store(group);
                session.Store(member);
                session.SaveChanges();

                Sender.Tell(new GroupCreatedEvent
                {
                    TransactionId = command.TransactionId,
                    Group = group
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }

        private static bool IsNewGroupValid(CreateGroupCommand command, IDocumentSession session)
        {
            var groupName = command.Name;

            // Note: RavenDB is case-insensitive while comparing strings
            // https://ravendb.net/docs/article-page/3.0/Csharp/indexes/using-analyzers
            var existingGroup = session.Query<Group>().FirstOrDefault(g => g.Name == groupName);

            return existingGroup == null;
        }

        private static string GenerateGroupIdAsync(IDocumentSession session)
        {
            while (true)
            {
                var newGroupId = Utils.GenerateRandomId("group");
                var existingGroup = session.Query<Group>().FirstOrDefault(g => g.Id == newGroupId);

                if (existingGroup == null)
                {
                    return newGroupId;
                }
            }
        }
    }
}