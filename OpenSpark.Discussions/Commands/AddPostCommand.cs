using OpenSpark.Domain;

namespace OpenSpark.Discussions.Commands
{
    public class AddPostCommand
    {
        public string PostType { get; }
        public Post Post { get; }

        public AddPostCommand(Post post)
        {
            Post = post;
        }
    }
}
