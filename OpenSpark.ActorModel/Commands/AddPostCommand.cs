using OpenSpark.Domain;

namespace OpenSpark.ActorModel.Commands
{
    public class AddPostCommand
    {
        public User User { get; }
        public Post Post { get; }

        public AddPostCommand(User user, Post post)
        {
            User = user;
            Post = post;
        }
    }
}
