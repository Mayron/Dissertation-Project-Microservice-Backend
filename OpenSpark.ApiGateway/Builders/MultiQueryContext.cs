using System;
using System.Collections.Generic;
using OpenSpark.Shared.Domain;

namespace OpenSpark.ApiGateway.Builders
{
    public class MultiQueryContext
    {
        public IList<QueryContext> Queries { get; set; }
        public User User { get; set; }
        public Guid Id { get; set; }
        public int TimeoutInSeconds { get; set; }
        public string Callback { get; set; }
        public string ConnectionId { get; set; }
        public string Aggregator { get; set; }
        public string Handler { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}