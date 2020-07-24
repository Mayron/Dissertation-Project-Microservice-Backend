using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class SearchGroupsQuery : IQuery
    {
        public User User { get; set; }
        public QueryMetaData MetaData { get; set; }
        public string SearchQuery { get; set; }
    }
}