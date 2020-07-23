using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Projects
{
    public class DeleteProjectCommand : ICommand
    {
        public string ProjectId { get; set; }
        public User User { get; set; }
    }
}