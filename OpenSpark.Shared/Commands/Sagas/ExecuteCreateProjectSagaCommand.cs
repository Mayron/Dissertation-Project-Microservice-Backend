using System;
using System.Collections.Generic;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.SagaExecutionCommands
{
    public class ExecuteCreateProjectSagaCommand : ISagaExecutionCommand
    {
        public Guid TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public List<string> Tags { get; set; }
    }
}
