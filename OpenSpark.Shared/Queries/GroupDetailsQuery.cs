using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class GroupDetailsQuery : IQuery
    {
        public User User { get; set; }
        public QueryMetaData MetaData { get; set; }
        public string GroupId { get; set; }
        public bool RetrieveGroupNameOnly { get; set; }
    }
}