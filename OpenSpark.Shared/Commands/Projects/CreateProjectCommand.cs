using OpenSpark.Domain;
using System.Collections.Generic;

namespace OpenSpark.Shared.Commands.Projects
{
    public class CreateProjectCommand : ICommand
    {
        public string Name { get; set; }
        public List<string> Tags { get; set; }
        public string About { get; set; }
        public User User { get; set; }
        public string Visibility { get; set; }
        public MetaData MetaData { get; set; }
    }
}