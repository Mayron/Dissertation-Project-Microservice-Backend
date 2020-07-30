using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Queries
{
    public class SearchGroupsQuery : IQuery
    {
        public User User { get; set; }
        public MetaData MetaData { get; set; }
        public string SearchQuery { get; set; }
    }
}