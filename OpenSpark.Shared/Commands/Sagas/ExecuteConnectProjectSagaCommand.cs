using System;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.SagaExecutionCommands;

namespace OpenSpark.Shared.Commands.Sagas
{
    public class ExecuteConnectProjectSagaCommand : ISagaExecutionCommand
    {
        public Guid TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
        public string ProjectId { get; set; }
        public string GroupId { get; set; }
    }
}