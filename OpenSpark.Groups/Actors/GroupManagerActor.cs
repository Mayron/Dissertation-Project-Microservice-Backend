using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using OpenSpark.Shared.Commands.Sagas;

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

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);
                if (!_children.Any(u => u.Value.Equals(terminated.ActorRef))) return;

                _children = _children.Where(u => !u.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
            });
        }

        private IActorRef GetGroupChildActor(string groupId)
        {
            if (_children.ContainsKey(groupId))
                return _children[groupId];

            var groupActor = Context.ActorOf(
                Props.Create(() => new GroupActor(groupId)), $"Group-{groupId}");

            Context.Watch(groupActor);
            _children = _children.Add(groupId, groupActor);

            return groupActor;
        }
    }
}