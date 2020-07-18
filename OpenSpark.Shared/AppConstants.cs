namespace OpenSpark.Shared
{
    public static class AppConstants
    {
        public static class Permissions
        {
            public const string CanPost = "7FE28F2A-D904-4A32-8033-1577C84EF078";
        }

        // Implicit Roles (calculated based on Members list and OwnerUserId)
        public static class ImplicitRoles
        {
            public const string OwnerRole = "B76CC994-F409-4860-9F5D-9825C763CDA9";
            public const string MembersRole = "BD44374A-6086-408C-A0C5-F7E0862C9BB1";
            public const string NonMembersRole = "DE48B38C-E233-4B0B-9267-944523366A81";
        }
    }
}