using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Projects
{
    public class ConnectProjectCommand : ICommand
    {
        public string ProjectId { get; set; }
        public string GroupId { get; set; }
        public string GroupVisibility { get; set; }

        // Can be Empty if not being used by a saga
        public Guid TransactionId { get; set; }
        public User User { get; set; }
    }
}