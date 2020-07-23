using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class ProjectDetailsQuery : IQuery
    {
        public string Callback { get; set; }
        public string ConnectionId { get; set; }
        public Guid MultiQueryId { get; set; }
        public Guid Id { get; set; }
        public string ProjectId { get; set; }
        public User User { get; set; }
        public bool RetrieveProjectNameOnly { get; set; }
    }
}
