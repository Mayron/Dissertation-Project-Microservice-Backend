using System;
using System.Collections.Generic;

namespace OpenSpark.Shared.Commands.Sagas.CreateGroup
{
    public class CreateGroupCommand : ISagaCommand
    {
        public Guid TransactionId { get; set; }
        public string Name { get; set; }
        public List<string> Connected { get; set; }
        public List<string> Tags { get; set; }
        public string CategoryId { get; set; }
        public string About { get; set; }
        public string OwnerUserId { get; set; }
    }
}