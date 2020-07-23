using Akka.Actor;
using OpenSpark.Shared.Commands.Posts;
using OpenSpark.Shared.Queries;

namespace OpenSpark.Discussions.Actors
{
    public class DiscussionManagerActor : ReceiveActor
    {
        public DiscussionManagerActor()
        {
            var createPostPool = Context.ActorOf(CreatePostActor.Props, "CreatePostPool");
            var newsFeedPool = Context.ActorOf(NewsFeedActor.Props, "NewsFeedPool");

            Receive<CreatePostCommand>(command =>
                createPostPool.Forward(command));

            Receive<NewsFeedQuery>(query =>
                newsFeedPool.Forward(query));
        }
    }
}