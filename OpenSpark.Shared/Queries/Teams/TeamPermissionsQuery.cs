using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Queries.Teams
{
    public class TeamPermissionsQuery : IQuery
    {
        public string TeamId { get; set; }
        public User User { get; set; }
        public MetaData MetaData { get; set; }
    }
}