using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class GroupPostQuery : IQuery
    {
        public User User { get; set; }
        public QueryMetaData MetaData { get; set; }
        public string GroupId { get; set; }
        public string PostId { get; set; }
    }
}