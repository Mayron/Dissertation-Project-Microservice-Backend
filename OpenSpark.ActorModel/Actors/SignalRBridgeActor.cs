using Akka.Actor;
using OpenSpark.ActorModel.Commands;
using OpenSpark.ActorModel.Services;

namespace OpenSpark.ActorModel.Actors
{
    // This class mixes the actor system with the signalR events
    public class SignalRBridgeActor : ReceiveActor
    {
        public SignalRBridgeActor(IEventEmitter eventEmitter, IActorRef userManager)
        {
            Receive<AddPostCommand>(userManager.Tell);

            Receive<AddChatMessageCommand>(command =>
            {
                eventEmitter.ChatMessageSent(command.User, command.ChatMessage);
            });

            Receive<ConnectUserCommand>(command =>
            {
                eventEmitter.UserConnected(command.User); // emit event to other users of groups/projects
                userManager.Tell(command); // tell user manager to store state
            });
        }
    }
}
