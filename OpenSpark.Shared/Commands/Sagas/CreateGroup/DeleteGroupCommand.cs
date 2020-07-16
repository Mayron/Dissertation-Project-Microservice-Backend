using System;

namespace OpenSpark.Shared.Commands.Sagas.CreateGroup
{
    public class DeleteGroupCommand : ISagaCommand
    {
        public Guid TransactionId { get; set; }
        public string GroupId { get; set; }
    }
}