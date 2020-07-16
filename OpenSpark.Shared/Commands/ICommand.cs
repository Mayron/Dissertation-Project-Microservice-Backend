using System;

namespace OpenSpark.Shared.Commands
{
    public interface ICommand : IMessage
    {
        Guid TransactionId { get; set; }
    }
}
