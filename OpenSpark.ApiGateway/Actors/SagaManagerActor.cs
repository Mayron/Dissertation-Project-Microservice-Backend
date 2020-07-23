using Akka.Actor;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace OpenSpark.ApiGateway.Actors
{
    public class SagaManagerActor : UntypedActor
    {
        private readonly IActorSystemService _actorSystemService;
        private readonly IFirestoreService _firestoreService;
        private IImmutableDictionary<Guid, IActorRef> _children;

        public SagaManagerActor(IActorSystemService actorSystemService,
            IFirestoreService firestoreService)
        {
            _actorSystemService = actorSystemService;
            _firestoreService = firestoreService;
            _children = ImmutableDictionary<Guid, IActorRef>.Empty;
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case ISagaExecutionCommand command:
                    var actorRef = GetChildActorRef(command);
                    actorRef?.Tell(command);
                    break;

                case Terminated terminated:
                    _children = _children.Where(c => !c.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
                    Context.Unwatch(terminated.ActorRef);
                    break;
            }
        }

        public IActorRef GetChildActorRef(ISagaExecutionCommand command)
        {
            if (_children.ContainsKey(command.TransactionId))
                return _children[command.TransactionId];

            var sagaActorRef = Context.Watch(CreateSagaActor(command));

            _children = _children.Add(command.TransactionId, sagaActorRef);
            _actorSystemService.RegisterTransaction(command.TransactionId);

            return sagaActorRef;
        }

        private IActorRef CreateSagaActor(ISagaExecutionCommand command)
        {
            var actorName = $"{command.SagaName}-{command.TransactionId}";

            return command.SagaName switch
            {
                nameof(CreatePostSagaActor) =>
                Context.ActorOf(
                    Props.Create(() => new CreatePostSagaActor(_actorSystemService)), actorName),

                nameof(CreateGroupSagaActor) =>
                Context.ActorOf(
                    Props.Create(() => new CreateGroupSagaActor(_actorSystemService, _firestoreService)), actorName),

                nameof(CreateProjectSagaActor) =>
                Context.ActorOf(
                    Props.Create(() => new CreateProjectSagaActor(_actorSystemService, _firestoreService)), actorName),

                nameof(ConnectProjectSagaActor) =>
                Context.ActorOf(
                    Props.Create(() => new ConnectProjectSagaActor(_actorSystemService)), actorName),

                _ => throw new Exception($"Failed to find SagaActor: {command.SagaName}"),
            };
        }
    }
}