using Akka.Actor;

namespace OpenSpark.ApiGateway.Services.SDK
{
    public interface ILocalActorSystemService
    {
        IActorRef CallbackActorRef { get; }
        ActorSystem LocalSystem { get; }
    }
}
