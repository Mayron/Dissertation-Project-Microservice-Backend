using OpenSpark.Domain;
using System;

namespace OpenSpark.Shared.Commands.SagaExecutionCommands
{
    public class ExecuteConnectProjectSagaCommand : ISagaExecutionCommand
    {
        public Guid TransactionId { get; set; }
        public User User { get; set; }
        public string SagaName { get; set; }
        public string ProjectId { get; set; }
        public string GroupId { get; set; }
    }
}