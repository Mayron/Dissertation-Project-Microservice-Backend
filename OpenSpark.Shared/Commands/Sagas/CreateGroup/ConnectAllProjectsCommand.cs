using System;
using System.Collections.Generic;

namespace OpenSpark.Shared.Commands.Sagas.CreateGroup
{
    public class ConnectAllProjectsCommand : ISagaCommand
    {
        public Guid TransactionId { get; set; }
        public string GroupId { get; set; }
        public List<string> ProjectIds { get; set; }
    }
}