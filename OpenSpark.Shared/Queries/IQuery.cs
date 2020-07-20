using System;

namespace OpenSpark.Shared.Queries
{
    public interface IQuery : IMessage
    {
        Guid Id { get; set; }
        string Callback { get; set; }
        string ConnectionId { get; set; }
    }
}