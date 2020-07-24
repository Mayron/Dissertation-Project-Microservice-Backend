namespace OpenSpark.Shared
{
    public static class AppConstants
    {
        public static class GroupPermissions
        {
            public const string CanPost = "CanPost";
            public const string CanViewSettings = "CanViewSettings";
            public const string CanDeleteGroup = "CanDeleteGroup";
            public const string CanEditAboutSection = "CanEditAboutSection";
            public const string CanEditGeneralSettings = "CanEditGeneralSettings";
            public const string CanEditRoleSettings = "CanEditRoleSettings";
            public const string CanBanMembers = "CanBanMembers";
            public const string CanChangeMemberRoles = "CanChangeMemberRoles";
            public const string CanApproveListedProjects = "CanApproveListedProjects";
        }

        // Implicit Roles (calculated based on Members list and OwnerUserId)
        public static class ImplicitGroupRoles
        {
            public const string OwnerRole = "B76CC994-F409-4860-9F5D-9825C763CDA9";
            public const string MemberRole = "BD44374A-6086-408C-A0C5-F7E0862C9BB1";
            public const string NonMembersRole = "DE48B38C-E233-4B0B-9267-944523366A81";
        }

        // Explicit Roles
        public static class ExplicitGroupRoles
        {
            public const string ModeratorRole = "922FE103-5131-4F56-9C74-4F7A3C2A754F";
        }

        public static class UserMessages
        {
            public const string GroupNameTaken = "Sorry, that group name has already been taken. Please choose a different name.";
        }
    }
}