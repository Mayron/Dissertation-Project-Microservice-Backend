using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Dispatch.SysMsg;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Sagas.CreatePost;
using OpenSpark.Shared.Events.Sagas;

namespace OpenSpark.Groups.Actors
{
    public class VerifyUserPostActor : ReceiveActor
    {
        private readonly Group _group;

        public VerifyUserPostActor(Group group)
        {
            _group = group;

            Receive<VerifyUserPostRequestCommand>(command =>
            {
                var verified = false;
                var bannedUser = _group.BannedUsers.FirstOrDefault(m => m.UserId == command.UserId);

                if (bannedUser != null)
                {
                    var member = _group.Members.FirstOrDefault(m => m.UserId == command.UserId);
                    var userIsGroupMember = member != null;

                    // Check if member has required CanPost permission
                    if (userIsGroupMember)
                    {
                        if (MemberHasRequiredPermission(member, AppConstants.Permissions.CanPost))
                        {
                            verified = true;
                        }
                    }
                    else if (_group.VisibilityStatus != VisibilityStatus.Private)
                    {
                        // user is treated as "Non-Members" Role
                        var nonMemberRole = _group.Roles.Single(r => r.Id == AppConstants.ImplicitRoles.NonMembersRole);

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

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }

        public bool MemberHasRequiredPermission(Member member, Guid permissionId)
        {
            return member.RoleIds.Any(roleId => _group.Roles.Single(r => r.Id == roleId)
                    .PermissionIds.Any(pid => pid == permissionId));
        }
    }
}
