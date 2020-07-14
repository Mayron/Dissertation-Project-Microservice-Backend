using Akka.Actor;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Commands.Sagas;

namespace OpenSpark.Discussions.Actors
{
    public class PostRepositoryActor : ReceiveActor
    {
        public PostRepositoryActor()
        {
            Receive<AddPostCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();
                session.Store(command.Post);
                session.SaveChanges();
            });
        }
    }
}
