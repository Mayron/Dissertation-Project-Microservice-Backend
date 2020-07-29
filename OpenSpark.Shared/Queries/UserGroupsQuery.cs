using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class UserGroupsQuery : IQuery
    {
        public User User { get; set; }
        public MetaData MetaData { get; set; }
        public bool Memberships { get; set; }
        public bool OwnedGroups { get; set; }
    }
}