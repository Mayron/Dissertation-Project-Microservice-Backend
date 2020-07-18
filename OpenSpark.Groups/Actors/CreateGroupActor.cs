using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Events.Sagas;
using OpenSpark.Shared.Events.Sagas.CreateGroup;
using OpenSpark.Shared.RavenDb;
using System;
using System.Collections.Generic;

namespace OpenSpark.Groups.Actors
{
    public class CreateGroupActor : ReceiveActor
    {
        public CreateGroupActor()
        {
            Receive<CreateGroupCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                if (session.IsNameTaken<Group>(command.Name))
                {
                    Sender.Tell(new SagaErrorEvent
                    {
                        Message = "Group name taken",
                        TransactionId = command.TransactionId
                    });

                    Self.GracefulStop(TimeSpan.FromSeconds(5));
                    return;
                }

                var groupId = session.GenerateRavenId<Group>();
                var memberId = session.GenerateRavenId<Member>();

                var member = new Member
                {
                    GroupId = groupId,
                    Id = memberId,
                    AuthUserId = command.User.AuthUserId,
                    Joined = DateTime.Now,
                };

                var group = new Group
                {
                    Id = groupId,
                    OwnerUserId = command.User.AuthUserId,
                    Members = new List<string> { memberId },
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
    }
}