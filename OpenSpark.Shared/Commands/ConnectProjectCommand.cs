using System;
using OpenSpark.Shared.Commands.Sagas;

namespace OpenSpark.Shared.Commands
{
    public class ConnectProjectCommand : ISagaCommand
    {
        public string ProjectId { get; set; }
        public string GroupId { get; set; }
        public string GroupVisibilityStatus { get; set; }

        // Can be Empty if not being used by a saga
        public Guid TransactionId { get; set; }
    }
}