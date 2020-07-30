using OpenSpark.Shared;
using System.Collections.Generic;

namespace OpenSpark.Teams
{
    public static class TeamPermissionsHelper
    {
        public static HashSet<string> GetDefaultNewTeamPermissions()
        {
            return new HashSet<string>
            {
                AppConstants.TeamPermissions.CanReadChatChannels,
                AppConstants.TeamPermissions.CanWriteChatChannels,
            };
        }

        public static HashSet<string> GetDefaultModeratorPermissions()
        {
            return new HashSet<string>(GetDefaultNewTeamPermissions())
            {
                AppConstants.TeamPermissions.CanCloseIssues,
                AppConstants.TeamPermissions.CanEditIssueTags,
                AppConstants.TeamPermissions.CanUploadFiles,
                AppConstants.TeamPermissions.CanListProjectOnGroups,
                AppConstants.TeamPermissions.CanManageKnowledgeBase,
                AppConstants.TeamPermissions.CanManageProjectPages,
                AppConstants.TeamPermissions.CanViewTeamSettings
            };
        }

        public static HashSet<string> GetDefaultAdminPermissions()
        {
            return new HashSet<string>(GetDefaultModeratorPermissions())
            {
                AppConstants.TeamPermissions.CanApproveFiles,
                AppConstants.TeamPermissions.CanCreateChatChannels,
                AppConstants.TeamPermissions.CanPublishOpportunities,
                AppConstants.TeamPermissions.CanEditTeamSettings
            };
        }

        //        public static bool HasRequiredGroupPermission(this Member member, Group group, string requiredPermissionId) =>
        //            member.RoleIds.Any(roleId => group.Roles.Single(r => r.Id == roleId)
        //                .PermissionIds.Any(pid => pid == requiredPermissionId));
    }
}