using System;
using System.Collections.Generic;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Projects
{
    public class CreateProjectCommand : ICommand
    {
        public Guid TransactionId { get; set; }
        public string Name { get; set; }
        public List<string> Tags { get; set; }
        public string About { get; set; }
        public User User { get; set; }
    }
}