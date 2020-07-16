using System;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Commands.SagaExecutionCommands;

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
                case ICommand command:
                    var actorRef = GetChildActorRef(command);
                    actorRef?.Tell(command);
                    break;
                
                case Terminated terminated:
                    _children = _children.Where(c => !c.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
                    Context.Unwatch(terminated.ActorRef);
                    break;
            }
        }

        public IActorRef GetChildActorRef(ICommand command)
        {
            if (_children.ContainsKey(command.TransactionId))
                return _children[command.TransactionId];

            if (command is ISagaExecutionCommand executionCommand)
            {
                var actorRef = _actorSystemService.CreateSagaActor(executionCommand);
                Context.Watch(actorRef);

                _children = _children.Add(command.TransactionId, actorRef);
                return actorRef;
            }

            Console.WriteLine($"Failed to get child actor for command: {command.GetType()}");
            return null;
        }
    }
}
