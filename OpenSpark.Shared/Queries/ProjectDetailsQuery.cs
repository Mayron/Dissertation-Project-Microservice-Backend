using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class ProjectDetailsQuery : IQuery
    {
        public string ConnectionId { get; set; }
        public User User { get; set; }
        public string Callback { get; set; }
        public string ProjectId { get; set; }
    }
}
