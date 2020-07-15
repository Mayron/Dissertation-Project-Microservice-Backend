using System;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.Sagas;

namespace OpenSpark.ApiGateway.Actors
{
    public class SagaManagerActor : UntypedActor
    {
        private readonly IActorSystemService _actorSystemService;
        private IImmutableDictionary<Guid, IActorRef> _children;

        public SagaManagerActor(IActorSystemService actorSystemService)
        {
            _actorSystemService = actorSystemService;
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

            var actorRef = _actorSystemService.CreateAddPostSagaActor(id);

            Context.Watch(actorRef);

            _children = _children.Add(id, actorRef);
            return actorRef;
        }
    }
}
