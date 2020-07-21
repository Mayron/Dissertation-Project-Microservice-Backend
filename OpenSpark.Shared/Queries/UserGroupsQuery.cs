using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class UserGroupsQuery : IQuery
    {
        public bool Memberships { get; set; }
        public bool OwnedGroups { get; set; }
        public string Callback { get; set; }
        public string ConnectionId { get; set; }
        public Guid MultiQueryId { get; set; }
        public Guid Id { get; set; }
        public User User { get; set; }
    }
}
