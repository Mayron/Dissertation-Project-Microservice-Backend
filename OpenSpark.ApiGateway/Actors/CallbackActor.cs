using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Events;

namespace OpenSpark.ApiGateway.Actors
{
    /// <summary>
    /// A callback actor for other actor systems to contact with results when ready.
    /// </summary>
    public class CallbackActor : ReceiveActor
    {
        public CallbackActor(IEventEmitterService eventEmitter)
        {
            Receive<NewsFeedReceivedEvent>(eventEmitter.ReceivedPayload);
        }
    }
}
