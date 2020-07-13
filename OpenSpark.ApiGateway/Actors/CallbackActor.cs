using Akka.Actor;
using OpenSpark.ApiGateway.Services.SDK;
using OpenSpark.Shared.Payloads;

namespace OpenSpark.ApiGateway.Actors
{
    /// <summary>
    /// A callback actor for other actor systems to contact with results when ready.
    /// </summary>
    public class CallbackActor : ReceiveActor
    {
        public CallbackActor(IEventEmitterService eventEmitter)
        {
            Receive<NewsFeedPostsPayload>(eventEmitter.ReceivedPayload);
        }
    }
}
