using System;

namespace OpenSpark.Shared
{
    public class MetaData
    {
        public Guid Id { get; set; }
        public string Callback { get; set; }
        public string ConnectionId { get; set; }
        // Optional - used for saga transactions and multi-queries
        public Guid ParentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}