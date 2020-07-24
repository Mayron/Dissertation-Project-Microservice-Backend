using System;

namespace OpenSpark.Shared.Commands.SagaExecutionCommands
{
    public interface ISagaExecutionCommand : ICommand
    {
        Guid TransactionId { get; set; }
        DateTime CreatedAt { get; set; }
    }
}