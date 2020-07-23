using System;
using System.Collections.Generic;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class MultiQuery
    {
        public IList<QueryContext> Queries { get; set; }
        public User User { get; set; }
        public Guid Id { get; set; }
        public int TimeOutInSeconds { get; set; } = 8;
        public string Callback { get; set; }
        public string ConnectionId { get; set; }
        public string Aggregator { get; set; }
        public string Handler { get; set; }
    }
}