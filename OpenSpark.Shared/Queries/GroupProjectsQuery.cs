using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class GroupProjectsQuery : IQuery
    {
        public string Callback { get; set; }
        public string ConnectionId { get; set; }
        public Guid MultiQueryId { get; set; }
        public Guid Id { get; set; }
        public string GroupId { get; set; }
        public User User { get; set; }
    }
}
