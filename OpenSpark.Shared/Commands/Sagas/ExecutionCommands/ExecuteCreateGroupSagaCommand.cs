using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Sagas.ExecutionCommands
{
    public class ExecuteCreateGroupSagaCommand : ISagaExecutionCommand
    {
        public Guid TransactionId { get; set; }
        public Group Group { get; set; }
        public User User { get; set; }
        public string SagaName { get; set; }
    }
}
