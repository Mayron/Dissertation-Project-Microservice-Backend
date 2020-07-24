using System;

namespace OpenSpark.Shared.Queries
{
    public class QueryMetaData
    {
        public string Callback { get; set; }
        public string ConnectionId { get; set; }
        public Guid MultiQueryId { get; set; }
        public Guid QueryId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}