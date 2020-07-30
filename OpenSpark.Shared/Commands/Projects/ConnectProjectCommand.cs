using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Commands.Projects
{
    public class ConnectProjectCommand : ICommand
    {
        public string ProjectId { get; set; }
        public string GroupId { get; set; }
        public string GroupVisibility { get; set; }
        public User User { get; set; }
        public MetaData MetaData { get; set; }
    }
}