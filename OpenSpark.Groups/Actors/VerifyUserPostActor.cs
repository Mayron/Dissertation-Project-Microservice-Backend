using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Posts;
using OpenSpark.Shared.Events.Sagas;
using OpenSpark.Shared.Events.Sagas.CreatePost;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSpark.Groups.Actors
{
    public class VerifyUserPostActor : ReceiveActor
    {
        private readonly GroupRepository _groupRepository;
        private readonly string _groupId;

        public VerifyUserPostActor(string groupId, GroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
            _groupId = groupId;

            Receive<VerifyUserPostRequestCommand>(HandleVerifyUserPostRequestCommand);
        }

        private void HandleVerifyUserPostRequestCommand(VerifyUserPostRequestCommand command)
        {
            var verified = false;
            var userId = command.User.AuthUserId;
            var group = FetchGroupDocument(command.TransactionId);

            if (group.BannedUsers.Contains(userId))
            {
                // Check if member has required CanPost permission
                if (group.Members.Contains(userId))
                {
                    var member = _groupRepository.GetGroupMember(userId, _groupId);

                    if (member == null)
                        Console.WriteLine($"Failed to find member {userId} for group {_groupId}");
                    else if (MemberHasRequiredPermission(group.Roles, member, AppConstants.Permissions.CanPost))
                        verified = true;
                }
                else if (group.Visibility != VisibilityStatus.Private)
                {
                    // user is treated as "Non-Members" Role
                    var nonMemberRole = group.Roles.Single(r => r.Id == AppConstants.ImplicitRoles.NonMembersRole);

                    // Check if user can post to this public/unlisted group
                    if (nonMemberRole.PermissionIds.Any(pid => pid == AppConstants.Permissions.CanPost))
                        verified = true;
                }
            }

            if (verified)
                Sender.Tell(new UserVerifiedEvent { TransactionId = command.TransactionId });
            else
                Sender.Tell(new UserVerificationFailedEvent { TransactionId = command.TransactionId });

            Self.GracefulStop(TimeSpan.FromSeconds(5));
        }

        public bool MemberHasRequiredPermission(List<Role> roles, Member member, Guid permissionId)
        {
            return member.RoleIds.Any(roleId => roles.Single(r => r.Id == roleId)
                    .PermissionIds.Any(pid => pid == permissionId));
        }

        private Group FetchGroupDocument(Guid transactionId)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var group = session.Query<Group>().SingleOrDefault(g => g.GroupId == _groupId);
            if (group != null) return group;

            var message = $"Failed to retrieve group: {_groupId}";

            Sender.Tell(new SagaErrorEvent
            {
                Message = message,
                TransactionId = transactionId
            });

            throw new ActorKilledException(message);
        }
    }
}