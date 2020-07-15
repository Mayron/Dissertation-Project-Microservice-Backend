using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Commands.Sagas.CreatePost;

namespace OpenSpark.Discussions.Actors
{
    public class UserActor : ReceiveActor
    {
        private readonly string _connectionId;
        private readonly User _user;

        public UserActor(string connectionId, User user)
        {
            _connectionId = connectionId;
            _user = user;

            Receive<CreatePostCommand>(handler =>
            {
                // handle this command!
                // can change state, or something else

                // TODO: Should I Sender.Tell("some event occured!"); ??
            });

            Receive<FetchNewsFeedCommand>(command =>
            {
                var newsFeedActor = Context.ActorOf(
                    Props.Create(() => new NewsFeedActor(_user)));

                newsFeedActor.Forward(command);
            });
        }
    }
}
