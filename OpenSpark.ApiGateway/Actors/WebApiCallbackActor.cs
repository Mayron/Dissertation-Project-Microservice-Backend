using Akka.Actor;
using OpenSpark.ApiGateway.Services.SDK;
using OpenSpark.Discussions.Payloads;

namespace OpenSpark.ApiGateway.Actors
{
    /// <summary>
    /// A callback actor for other actor systems to contact with results when ready.
    /// </summary>
    public class WebApiCallbackActor : ReceiveActor
    {
        public WebApiCallbackActor(IEventEmitterService eventEmitter)
        {
            Receive<NewsFeedPostsPayload>(eventEmitter.ReceivedPayload);
        }
    }
}
