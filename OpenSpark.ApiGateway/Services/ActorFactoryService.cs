using System;
using Akka.Actor;
using OpenSpark.ApiGateway.Actors;
using OpenSpark.ApiGateway.Actors.Sagas;

namespace OpenSpark.ApiGateway.Services
{
    public interface IActorFactoryService
    {
        IActorRef CreateAddPostSagaActor(Guid key);
        IActorRef CreateCallbackActor();
        IActorRef CreateSagaManagerActor();
    }

    public class ActorFactoryService : IActorFactoryService
    {
        private readonly IActorSystemService _actorSystemService;
        private readonly IEventEmitterService _eventEmitterService;

        public ActorFactoryService(IActorSystemService actorSystemService, IEventEmitterService eventEmitterService)
        {
            _actorSystemService = actorSystemService;
            _eventEmitterService = eventEmitterService;
        }

        public IActorRef CreateAddPostSagaActor(Guid key) =>
            _actorSystemService.LocalSystem.ActorOf(
                Props.Create(() => new AddPostSagaActor(_actorSystemService, _eventEmitterService)), 
                $"AddPostSagaActor-${key}");

        public IActorRef CreateCallbackActor() =>
            _actorSystemService.LocalSystem.ActorOf(
                Props.Create(() => new CallbackActor(_eventEmitterService)), "Callback");

        public IActorRef CreateSagaManagerActor() =>
            _actorSystemService.LocalSystem.ActorOf(
                Props.Create(() => new SagaManagerActor(this)), "SagaManager");
    }
}
