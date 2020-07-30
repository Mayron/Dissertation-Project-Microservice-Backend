using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Commands.Teams
{
    public class CreateTeamCommand : ICommand
    {
        public User User { get; set; }
        public MetaData MetaData { get; set; }
        public string TeamName { get; set; }
        public string ProjectId { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
    }
}