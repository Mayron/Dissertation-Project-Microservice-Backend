using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Queries
{
    public class TeamsQuery : IQuery
    {
        public string ProjectId { get; set; }
        public User User { get; set; }
        public MetaData MetaData { get; set; }
    }
}