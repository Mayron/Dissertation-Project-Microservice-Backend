using System;
using System.Collections.Generic;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Sagas.ExecutionCommands
{
    public class ExecuteCreateGroupSagaCommand : ISagaExecutionCommand
    {
        public Guid TransactionId { get; set; }
        public User User { get; set; }
        public string SagaName { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public string CategoryId { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Connecting { get; set; }
        public string OwnerUserId { get; set; }
    }
}
