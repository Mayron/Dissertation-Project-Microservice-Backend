using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Discussions
{
    public class VerifyPostRequestCommand : ICommand
    {
        public string GroupId { get; set; }
        public User User { get; set; }
        public MetaData MetaData { get; set; }
    }
}
