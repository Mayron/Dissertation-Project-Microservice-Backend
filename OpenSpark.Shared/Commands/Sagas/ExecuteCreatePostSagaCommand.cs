using System;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.SagaExecutionCommands;

namespace OpenSpark.Shared.Commands.Sagas
{
    public class ExecuteCreatePostSagaCommand : ISagaExecutionCommand
    {
        public Guid TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
        public string GroupId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
