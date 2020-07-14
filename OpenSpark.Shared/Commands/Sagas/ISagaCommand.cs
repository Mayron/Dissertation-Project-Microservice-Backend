using System;

namespace OpenSpark.Shared.Commands.Sagas
{
    public interface ISagaCommand : ICommand
    {
        Guid TransactionId { get; set; }
    }
}
