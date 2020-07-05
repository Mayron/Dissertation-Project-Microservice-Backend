using Akka.Actor;
using OpenSpark.ActorModel.Commands;
using OpenSpark.Domain;

namespace OpenSpark.ActorModel.Actors
{
    public class UserActor : ReceiveActor
    {
        private readonly User _user;
        
        public UserActor(User user)
        {
            _user = user;

            Receive<AddPostCommand>(handler =>
            {
                // handle this command!
                // can change state, or something else

                // TODO: Should I Sender.Tell("some event occured!"); ??

                
            });
        }
    }
}
