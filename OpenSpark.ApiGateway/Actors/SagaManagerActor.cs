using System;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.Sagas;

namespace OpenSpark.ApiGateway.Actors
{
    public class SagaManagerActor : UntypedActor
    {
        private readonly IActorFactoryService _actorFactoryService;
        private IImmutableDictionary<Guid, IActorRef> _children;

        public SagaManagerActor(IActorFactoryService actorFactoryService)
        {
            _actorFactoryService = actorFactoryService;
            _children = ImmutableDictionary<Guid, IActorRef>.Empty;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case ISagaCommand command:
                    var actorRef = GetChildActorRef(command.TransactionId);
                    actorRef?.Tell(command);
                    break;
                
                case Terminated terminated:
                    _children = _children.Where(c => !c.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
                    Context.Unwatch(terminated.ActorRef);
                    break;
            }
        }

        public IActorRef GetChildActorRef(Guid id)
        {
            if (_children.ContainsKey(id))
                return _children[id];

            var actorRef = _actorFactoryService.CreateAddPostSagaActor(id);
            Context.Watch(actorRef);

            _children = _children.Add(id, actorRef);
            return actorRef;
        }
    }
}
