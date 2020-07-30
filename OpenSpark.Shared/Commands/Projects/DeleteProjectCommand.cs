using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Commands.Projects
{
    public class DeleteProjectCommand : ICommand
    {
        public string ProjectId { get; set; }
        public User User { get; set; }
        public MetaData MetaData { get; set; }
    }
}