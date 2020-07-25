using Akka.Actor;
using Akka.Routing;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Posts;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Events.CreatePost;
using System;
using System.Linq;
using Group = OpenSpark.Domain.Group;

namespace OpenSpark.Groups.Actors
{
    public class VerifyPostActor : ReceiveActor
    {
        private readonly GroupRepository _groupRepository;

        public VerifyPostActor()
        {
            _groupRepository = new GroupRepository();
            Receive<VerifyPostRequestCommand>(HandleVerifyUserPostRequestCommand);
        }

        private void HandleVerifyUserPostRequestCommand(VerifyPostRequestCommand command)
        {
            var ravenGroupId = command.GroupId.ConvertToRavenId<Group>();
            var verified = false;
            var userId = command.User.AuthUserId;
            var group = FetchGroupDocument(ravenGroupId);

            if (group == null) return;

            if (!group.BannedUsers.Contains(userId))
            {
                // Check if member has required CanPost permission
                if (group.Members.Contains(userId))
                {
                    var member = _groupRepository.GetGroupMemberByAuthUserId(userId, ravenGroupId);

                    if (member == null)
                        Console.WriteLine($"Failed to find member {userId} for group {ravenGroupId}");
                    else if (member.HasRequiredGroupPermission(group, AppConstants.GroupPermissions.CanPost))
                        verified = true;
                }
                else if (group.Visibility != VisibilityStatus.Private)
                {
                    // user is treated as "Non-Members" Role
                    var nonMemberRole = group.Roles.Single(r => r.Id == AppConstants.ImplicitGroupRoles.NonMembersRole);

                    // Check if user can post to this public/unlisted group
                    if (nonMemberRole.PermissionIds.Any(pid => pid == AppConstants.GroupPermissions.CanPost))
                        verified = true;
                }
            }

            if (verified)
                Sender.Tell(new UserVerifiedEvent
                {
                    GroupName = group.Name,
                    GroupVisibility = group.Visibility
                });
            else
                Sender.Tell(new UserVerificationFailedEvent());
        }

        private Group FetchGroupDocument(string ravenGroupId)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var group = session.Load<Group>(ravenGroupId);
            if (group != null) return group;

            var message = $"Failed to retrieve group: {ravenGroupId}";

            Sender.Tell(new ErrorEvent
            {
                Message = message
            });

            Console.WriteLine(message);
            return null;
        }

        public static Props Props { get; } = Props.Create<VerifyPostActor>()
            .WithRouter(new RoundRobinPool(5,
                new DefaultResizer(1, 10)));
    }
}