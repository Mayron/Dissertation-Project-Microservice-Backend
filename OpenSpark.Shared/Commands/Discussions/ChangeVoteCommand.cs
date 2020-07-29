using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Discussions
{
    public class ChangeVoteCommand : ICommand
    {
        public User User { get; set; }
        public string CommentId { get; set; }
        public string PostId { get; set; }
        public int Amount { get; set; }
        public MetaData MetaData { get; set; }
    }
}