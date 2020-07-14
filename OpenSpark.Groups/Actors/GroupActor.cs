using System;
using System.Linq;
using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Events.Sagas;

namespace OpenSpark.Groups.Actors
{
    public class GroupActor : ReceiveActor
    {
        private readonly string _groupId;
        private readonly Lazy<Group> _state;

        private Group State => _state.Value;

        public GroupActor(string groupId)
        {
            _groupId = groupId;
            _state = new Lazy<Group>(FetchGroupDocument);

            Receive<VerifyUserPostRequestCommand>(command =>
            {
                var verified = false;
                var bannedUser = State.BannedUsers.FirstOrDefault(m => m.UserId == command.UserId);

                if (bannedUser != null)
                {
                    var member = State.Members.FirstOrDefault(m => m.UserId == command.UserId);
                    var userIsGroupMember = member != null;

                    // Check if member has required CanPost permission
                    if (userIsGroupMember)
                    {
                        if (MemberHasRequiredPermission(member, AppConstants.Permissions.CanPost))
                        {
                            verified = true;
                        }
                    }
                    else if (State.VisibilityStatus != VisibilityStatus.Private)
                    {
                        // user is treated as "Non-Members" Role
                        var nonMemberRole = State.Roles.Single(r => r.Id == AppConstants.NonMembersRole);

                        // Check if user can post to this public/unlisted group
                        if (nonMemberRole.PermissionIds.Any(pid => pid == AppConstants.Permissions.CanPost))
                        {
                            verified = true;
                        }
                    }
                }

                if (verified)
                {
                    Sender.Tell(new UserVerifiedEvent { TransactionId = command.TransactionId });
                }
                else
                {
                    Sender.Tell(new UserVerificationFailedEvent { TransactionId = command.TransactionId });
                }
            });
        }

        public bool MemberHasRequiredPermission(Member member, Guid permissionId)
        {
            return member.RoleIds.Any(roleId => State.Roles.Single(r => r.Id == roleId)
                    .PermissionIds.Any(pid => pid == permissionId));
        }

        public Group FetchGroupDocument()
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var group = session.Query<Group>().SingleOrDefault(g => g.GroupId == _groupId);

            if (group == null)
            {
                throw new Exception($"Failed to retrieve group: {_groupId}");
            }

            return group;
        }
    }
}