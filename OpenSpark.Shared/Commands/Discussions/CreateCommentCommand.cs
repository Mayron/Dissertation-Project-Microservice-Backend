using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Discussions
{
    public class CreateCommentCommand : ICommand
    {
        public User User { get; set; }
        public string Body { get; set; }
        public string PostId { get; set; }
        public MetaData MetaData { get; set; }
    }
}