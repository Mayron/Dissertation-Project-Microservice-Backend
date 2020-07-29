using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Events.Payloads;

namespace OpenSpark.ApiGateway.Actors
{
    /// <summary>
    /// A simple callback actor for other actor systems to contact with payload events when ready.
    /// Only usable for singular commands and queries that are not part of a Saga or MultiQuery.
    /// </summary>
    public class CallbackHandlerActor : ReceiveActor
    {
        public CallbackHandlerActor(IEventEmitter eventEmitter)
        {
            Receive<PayloadEvent>(eventEmitter.BroadcastToClient);
        }
    }
}