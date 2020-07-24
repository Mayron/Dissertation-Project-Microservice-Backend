using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class GroupProjectsQuery : IQuery
    {
        public User User { get; set; }
        public QueryMetaData MetaData { get; set; }
        public string GroupId { get; set; }
        public int TakeAmount { get; set; }
    }
}