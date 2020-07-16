using Akka.Actor;
using System.Collections.Immutable;
using System.Linq;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Commands.Posts;

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
                var actorRef = GetChildActor(command.GroupId);
                actorRef.Forward(command);
            });

            Receive<CreateGroupCommand>(command =>
            {
                var actorRef = GetChildActor(command.TransactionId.ToString());
                actorRef.Forward(command);
            });

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);
                if (!_children.Any(u => u.Value.Equals(terminated.ActorRef))) return;

                _children = _children.Where(u => !u.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
            });
        }

        private IActorRef GetChildActor(string id)
        {
            if (_children.ContainsKey(id))
                return _children[id];

            var childActor = Context.ActorOf(
                Props.Create(() => new GroupActor(id)), $"Group-{id}");

            Context.Watch(childActor);
            _children = _children.Add(id, childActor);

            return childActor;
        }
    }
}