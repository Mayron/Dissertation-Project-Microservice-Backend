using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Projects
{
    public class DeleteProjectCommand : ICommand
    {
        public Guid TransactionId { get; set; }
        public string ProjectId { get; set; }
        public User User { get; set; }
    }
}