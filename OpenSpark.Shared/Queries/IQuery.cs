using System;

namespace OpenSpark.Shared.Queries
{
    public interface IQuery : IMessage
    {
        string Callback { get; set; }
        string ConnectionId { get; set; }
        Guid MultiQueryId { get; set; }
        Guid Id { get; set; }
    }
}