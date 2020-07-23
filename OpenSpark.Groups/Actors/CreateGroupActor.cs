using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Events.CreateGroup;
using OpenSpark.Shared.RavenDb;
using System;
using System.Collections.Generic;
using Akka.Routing;
using Group = OpenSpark.Domain.Group;

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
                    Sender.Tell(new ErrorEvent
                    {
                        Message = "Group name taken"
                    });

                    return;
                }

                var groupId = session.GenerateRavenIdFromName<Group>(command.Name);
                var memberId = session.GenerateRavenId<Member>();

                var member = new Member
                {
                    GroupId = groupId,
                    Id = memberId,
                    AuthUserId = command.User.AuthUserId,
                    Joined = DateTime.Now,
                    RoleIds = new List<string> { AppConstants.ImplicitGroupRoles.OwnerRole }
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
                    ListedProjects = command.Connected,
                    Roles = RolesHelper.GetDefaultGroupRoles(),
                    BannedUsers = new List<string>(),
                    CreatedAt = DateTime.Now,
                };

                session.Store(group);
                session.Store(member);
                session.SaveChanges();

                Sender.Tell(new GroupCreatedEvent
                {
                    Group = group
                });
            });
        }

        public static Props Props { get; } = Props.Create<CreateGroupActor>()
            .WithRouter(new RoundRobinPool(1,
                new DefaultResizer(1, 5)));
    }
}