using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Events.Sagas;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Events.Sagas.CreateGroup;

namespace OpenSpark.Groups.Actors
{
    public class CreateGroupActor : ReceiveActor
    {
        public CreateGroupActor()
        {
            Receive<CreateGroupCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                if (!IsNewGroupValid(command, session))
                {
                    Sender.Tell(new SagaErrorEvent
                    {
                        Message = "Group name taken",
                        TransactionId = command.TransactionId
                    });

                    Self.GracefulStop(TimeSpan.FromSeconds(5));
                    return;
                }

                var group = new Group
                {
                    GroupId = GenerateGroupId(session),
                    OwnerUserId = command.OwnerUserId,
                    Members = new List<Member> { new Member { UserId = command.OwnerUserId } },
                    About = command.About,
                    Name = command.Name,
                    CategoryId = command.CategoryId,
                    Tags = command.Tags,
                    Visibility = VisibilityStatus.Public, // TODO: Needs to be configurable on creation
                    Connected = command.Connected,
                    Roles = new List<Role>(),
                    BannedUsers = new List<Member>(),
                    CreatedAt = DateTime.Now,
                };

                session.Store(group);
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
            var existingGroup = session.Query<Group>()
                .FirstOrDefault(g => g.Name == groupName);

            return existingGroup == null;
        }

        private static string GenerateGroupId(IDocumentSession session)
        {
            while (true)
            {
                var newGroupId = Utils.GenerateRandomId();
                var existingGroup = session.Query<Group>().FirstOrDefault(g => g.GroupId == newGroupId);

                if (existingGroup == null)
                {
                    return newGroupId;
                }
            }
        }
    }
}