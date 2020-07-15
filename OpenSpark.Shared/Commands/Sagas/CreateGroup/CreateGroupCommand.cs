using System;

namespace OpenSpark.Shared.Commands.Sagas.CreateGroup
{
    public class CreateGroupCommand : ISagaCommand
    {
        public Guid TransactionId { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
    }
}