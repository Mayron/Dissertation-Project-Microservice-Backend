using OpenSpark.Groups.Domain;
using OpenSpark.Shared;
using System.Collections.Generic;
using System.Linq;

namespace OpenSpark.Groups
{
    public static class GroupRolesHelper
    {
        public static List<Role> GetDefaultGroupRoles()
        {
            var nonMember = new Role
            {
                Id = AppConstants.ImplicitGroupRoles.NonMembersRole,
                Name = "Non-Member",
                PermissionIds = new HashSet<string>
                {
                    AppConstants.GroupPermissions.CanPost
                }
            };

            var member = new Role
            {
                Id = AppConstants.ImplicitGroupRoles.MemberRole,
                Name = "Member",
                PermissionIds = new HashSet<string>(nonMember.PermissionIds)
            };

            var moderator = new Role
            {
                Id = AppConstants.ExplicitGroupRoles.ModeratorRole,
                Name = "Moderator",
                PermissionIds = new HashSet<string>(member.PermissionIds)
                {
                    AppConstants.GroupPermissions.CanViewSettings,
                    AppConstants.GroupPermissions.CanChangeMemberRoles,
                    AppConstants.GroupPermissions.CanApproveListedProjects,
                }
            };

            var owner = new Role
            {
                Id = AppConstants.ImplicitGroupRoles.OwnerRole,
                Name = "Owner",
                PermissionIds = new HashSet<string>(moderator.PermissionIds)
                {
                    AppConstants.GroupPermissions.CanDeleteGroup,
                    AppConstants.GroupPermissions.CanEditAboutSection,
                    AppConstants.GroupPermissions.CanEditGeneralSettings,
                    AppConstants.GroupPermissions.CanEditRoleSettings,
                    AppConstants.GroupPermissions.CanBanMembers,
                }
            };

            return new List<Role> { owner, moderator, member, nonMember };
        }

        public static bool HasRequiredGroupPermission(this Member member, Group group, string requiredPermissionId) =>
            member.RoleIds.Any(roleId => group.Roles.Single(r => r.Id == roleId)
                .PermissionIds.Any(pid => pid == requiredPermissionId));
    }
}