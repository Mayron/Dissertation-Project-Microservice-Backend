using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class UserProjectsQuery : IQuery
    {
        public User User { get; set; }
        public QueryMetaData MetaData { get; set; }
        public bool OwnedProjects { get; set; }
        public bool Subscriptions { get; set; }
    }
}