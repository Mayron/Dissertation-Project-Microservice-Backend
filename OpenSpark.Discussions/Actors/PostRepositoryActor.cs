using Akka.Actor;
using OpenSpark.Shared.Commands;

namespace OpenSpark.Discussions.Actors
{
    public class PostRepositoryActor : ReceiveActor
    {
        public PostRepositoryActor()
        {
            Receive<AddPostCommand>(command =>
            {
                using var session = DatabaseSingleton.Store.OpenSession();
                session.Store(command.Post);
                session.SaveChanges();
            });
        }
    }
}
