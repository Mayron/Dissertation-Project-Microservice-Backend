using Akka.Actor;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace OpenSpark.ApiGateway.Actors
{
    public class SagaManagerActor : UntypedActor
    {
        private readonly IActorSystem _actorSystem;
        private readonly IFirestoreService _firestoreService;
        private IImmutableDictionary<Guid, IActorRef> _children;

        public SagaManagerActor(IActorSystem actorSystem,
            IFirestoreService firestoreService)
        {
            _actorSystem = actorSystem;
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
            if (_children.ContainsKey(context.Id))
                return _children[context.Id];

            var sagaActorRef = Context.Watch(CreateSagaActor(context));

            _children = _children.Add(context.Id, sagaActorRef);

            return sagaActorRef;
        }

        private IActorRef CreateSagaActor(SagaContext command)
        {
            var actorName = $"{command.SagaName}-{command.Id}";

            return command.SagaName switch
            {
                nameof(CreatePostSaga) =>
                Context.ActorOf(
                    Props.Create(() => new CreatePostSaga(_actorSystem)), actorName),

                nameof(CreateGroupSaga) =>
                Context.ActorOf(
                    Props.Create(() => new CreateGroupSaga(_actorSystem, _firestoreService)), actorName),

                nameof(CreateProjectSaga) =>
                Context.ActorOf(
                    Props.Create(() => new CreateProjectSaga(_actorSystem, _firestoreService)), actorName),

                nameof(ConnectProjectSaga) =>
                Context.ActorOf(
                    Props.Create(() => new ConnectProjectSaga(_actorSystem)), actorName),

                _ => throw new Exception($"Failed to find SagaActor: {command.SagaName}"),
            };
        }
    }
}