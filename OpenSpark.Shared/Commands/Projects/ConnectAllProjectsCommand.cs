using System.Collections.Generic;
using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Commands.Projects
{
    public class ConnectAllProjectsCommand : ICommand
    {
        public string GroupId { get; set; }
        public List<string> ProjectIds { get; set; }
        public User User { get; set; }
        public string GroupVisibility { get; set; }
        public MetaData MetaData { get; set; }
    }
}