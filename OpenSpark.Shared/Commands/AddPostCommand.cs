using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands
{
    public class AddPostCommand : ICommand
    {
        public Post Post { get; set; }
        public string GroupId { get; set; }

        public AddPostCommand()
        {
        }
    }
}
