using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands
{
    public class CreateCommentCommand : ICommand
    {
        public User User { get; set; }
        public string Body { get; set; }
        public string PostId { get; set; }
    }
}