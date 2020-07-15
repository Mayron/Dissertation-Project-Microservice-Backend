using Akka.Actor;
using OpenSpark.Shared.Commands.Sagas.CreatePost;

namespace OpenSpark.Discussions.Actors
{
    public class PostRepositoryActor : ReceiveActor
    {
        public PostRepositoryActor()
        {
            Receive<CreatePostCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();
                session.Store(command.Post);
                session.SaveChanges();
            });
        }
    }
}