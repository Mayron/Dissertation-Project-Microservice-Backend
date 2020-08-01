using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Commands.Teams
{
    public class ChangePermissionCommand : ICommand
    {
        public string TeamId { get; set; }
        public bool Enabled { get; set; }
        public string Permission { get; set; }
        public User User { get; set; }
        public MetaData MetaData { get; set; }
    }
}