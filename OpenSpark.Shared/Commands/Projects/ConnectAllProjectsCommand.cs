using System;
using System.Collections.Generic;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Projects
{
    public class ConnectAllProjectsCommand : ICommand
    {
        public Guid TransactionId { get; set; }
        public string GroupId { get; set; }
        public List<string> ProjectIds { get; set; }
        public User User { get; set; }
    }
}