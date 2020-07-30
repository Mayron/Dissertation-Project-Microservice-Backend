using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Commands.Teams
{
    public class CreateDefaultTeamsCommand : ICommand
    {
        public User User { get; set; }
        public MetaData MetaData { get; set; }
        public string ProjectId { get; set; }
    }
}