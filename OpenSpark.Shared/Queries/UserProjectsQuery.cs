using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class UserProjectsQuery : IQuery
    {
        public bool OwnedProjects { get; set; }
        public bool Subscriptions { get; set; }
        public Guid Id { get; set; }
        public string Callback { get; set; }
        public string ConnectionId { get; set; }
        public User User { get; set; }
    }
}
