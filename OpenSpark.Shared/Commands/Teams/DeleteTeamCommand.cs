using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Commands.Teams
{
    public class DeleteTeamCommand : ICommand
    {
        public User User { get; set; }
        public MetaData MetaData { get; set; }
        public string TeamId { get; set; }
    }
}