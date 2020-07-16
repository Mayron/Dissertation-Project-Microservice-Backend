using Akka.Actor;
using OpenSpark.Shared.Commands.Sagas.CreateGroup;
using OpenSpark.Shared.Commands.Sagas.CreatePost;
using System.Collections.Immutable;
using System.Linq;

namespace OpenSpark.Groups.Actors
{
    public class GroupManagerActor : ReceiveActor
    {
        private IImmutableDictionary<string, IActorRef> _children;

        public GroupManagerActor()
        {
            _children = ImmutableDictionary<string, IActorRef>.Empty;

            Receive<VerifyUserPostRequestCommand>(command =>
            {
                var actorRef = GetGroupChildActor(command.GroupId);
                actorRef.Forward(command);
            });

            Receive<CreateGroupCommand>(command =>
            {
                var actorRef = GetGroupChildActor(command.TransactionId.ToString());
                actorRef.Forward(command);
            });

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);
                if (!_children.Any(u => u.Value.Equals(terminated.ActorRef))) return;

                _children = _children.Where(u => !u.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
            });
        }

        private IActorRef GetGroupChildActor(string id)
        {
            if (_children.ContainsKey(id))
                return _children[id];

            var groupActor = Context.ActorOf(
                Props.Create(() => new GroupActor(id)), $"Group-{id}");

            Context.Watch(groupActor);
            _children = _children.Add(id, groupActor);

            return groupActor;
        }
    }
}