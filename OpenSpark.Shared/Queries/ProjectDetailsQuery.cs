using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Queries
{
    public class ProjectDetailsQuery : IQuery
    {
        public User User { get; set; }
        public MetaData MetaData { get; set; }
        public bool RetrieveProjectNameOnly { get; set; }
        public string ProjectId { get; set; }
    }
}