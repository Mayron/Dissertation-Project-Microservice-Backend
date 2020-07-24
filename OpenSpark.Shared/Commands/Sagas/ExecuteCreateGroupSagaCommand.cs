using System;
using System.Collections.Generic;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.SagaExecutionCommands;

namespace OpenSpark.Shared.Commands.Sagas
{
    public class ExecuteCreateGroupSagaCommand : ISagaExecutionCommand
    {
        public Guid TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public string CategoryId { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Connecting { get; set; }
    }
}
