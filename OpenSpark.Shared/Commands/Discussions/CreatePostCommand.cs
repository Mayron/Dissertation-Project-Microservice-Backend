using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Commands.Discussions
{
    public class CreatePostCommand : ICommand
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string GroupId { get; set; }
        public User User { get; set; }
        public string GroupVisibility { get; set; }
        public MetaData MetaData { get; set; }
    }
}