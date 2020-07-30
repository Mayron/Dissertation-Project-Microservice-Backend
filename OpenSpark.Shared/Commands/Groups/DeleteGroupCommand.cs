using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Commands.Groups
{
    public class DeleteGroupCommand : ICommand
    {
        public string GroupId { get; set; }
        public User User { get; set; }
        public MetaData MetaData { get; set; }
    }
}