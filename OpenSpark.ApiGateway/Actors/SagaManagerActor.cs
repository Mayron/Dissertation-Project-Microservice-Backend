using System;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Commands.SagaExecutionCommands;

namespace OpenSpark.ApiGateway.Actors
{
    public class SagaManagerActor : UntypedActor
    {
        private readonly IActorSystemService _actorSystemService;
        private readonly IEventEmitterService _eventEmitter;
        private readonly IFirestoreService _firestoreService;
        private IImmutableDictionary<Guid, IActorRef> _children;

        public SagaManagerActor(IActorSystemService actorSystemService, 
            IEventEmitterService eventEmitter, 
            IFirestoreService firestoreService)
        {
            _actorSystemService = actorSystemService;
            _eventEmitter = eventEmitter;
            _firestoreService = firestoreService;
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
                var actorRef = Context.Watch(CreateSagaActor(executionCommand));

                _children = _children.Add(command.TransactionId, actorRef);
                _actorSystemService.RegisterTransaction(command.TransactionId);

                return actorRef;
            }

            Console.WriteLine($"Failed to get child actor for command: {command.GetType()}");
            return null;
        }

        private IActorRef CreateSagaActor(ISagaExecutionCommand command)
        {
            var actorName = $"{command.SagaName}-{command.TransactionId}";

            return command.SagaName switch
            {
                nameof(CreatePostSagaActor) =>
                Context.ActorOf(
                    Props.Create(() => new CreatePostSagaActor(_actorSystemService, _eventEmitter)), actorName),

                nameof(CreateGroupSagaActor) =>
                Context.ActorOf(
                    Props.Create(() => new CreateGroupSagaActor(_actorSystemService, _firestoreService)), actorName),

                nameof(CreateProjectSagaActor) =>
                Context.ActorOf(
                    Props.Create(() => new CreateProjectSagaActor(_actorSystemService, _firestoreService)), actorName),

                _ => throw new Exception($"Failed to find SagaActor: {command.SagaName}"),
            };
        }
    }
}
