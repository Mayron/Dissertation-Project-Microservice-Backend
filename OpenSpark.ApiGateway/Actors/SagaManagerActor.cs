using Akka.Actor;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.Services;
using System;
using System.Collections.Immutable;
using System.Linq;
using OpenSpark.ApiGateway.Builders;

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
                case SagaContext context:
                    var actorRef = GetChildActorRef(context);
                    actorRef?.Tell(context.Command);
                    break;

                case Terminated terminated:
                    _children = _children.Where(c => !c.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
                    Context.Unwatch(terminated.ActorRef);
                    break;
            }
        }

        public IActorRef GetChildActorRef(SagaContext context)
        {
            if (_children.ContainsKey(context.TransactionId))
                return _children[context.TransactionId];

            var sagaActorRef = Context.Watch(CreateSagaActor(context));

            _children = _children.Add(context.TransactionId, sagaActorRef);
            _actorSystemService.RegisterTransaction(context.TransactionId);

            return sagaActorRef;
        }

        private IActorRef CreateSagaActor(SagaContext command)
        {
            var actorName = $"{command.SagaName}-{command.TransactionId}";

            return command.SagaName switch
            {
                nameof(CreatePostSaga) =>
                Context.ActorOf(
                    Props.Create(() => new CreatePostSaga(_actorSystemService)), actorName),

                nameof(CreateGroupSaga) =>
                Context.ActorOf(
                    Props.Create(() => new CreateGroupSaga(_actorSystemService, _firestoreService)), actorName),

                nameof(CreateProjectSaga) =>
                Context.ActorOf(
                    Props.Create(() => new CreateProjectSaga(_actorSystemService, _firestoreService)), actorName),

                nameof(ConnectProjectSaga) =>
                Context.ActorOf(
                    Props.Create(() => new ConnectProjectSaga(_actorSystemService)), actorName),

                _ => throw new Exception($"Failed to find SagaActor: {command.SagaName}"),
            };
        }
    }
}