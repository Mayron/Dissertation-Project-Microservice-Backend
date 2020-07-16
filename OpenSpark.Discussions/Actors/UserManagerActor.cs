using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Queries;
using System.Collections.Immutable;
using System.Linq;

namespace OpenSpark.Discussions.Actors
{
    // This parent actor needs to handle all requests coming in and forward them to the correct userActor
    public class UserManagerActor : ReceiveActor
    {
        private IImmutableDictionary<string, IActorRef> _children;

        public UserManagerActor()
        {
            _children = ImmutableDictionary<string, IActorRef>.Empty;

            //            Receive<AddPostCommand>(command =>
            //            {
            //                _users[command.ConnectionId].Forward(command);
            //            });

            Receive<NewsFeedQuery>(query =>
            {
                var actorRef = GetChildActor(query.ConnectionId, query.User);
                actorRef.Forward(query);
            });

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);
                if (!_children.Any(u => u.Value.Equals(terminated.ActorRef))) return;

                _children = _children.Where(u => !u.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
            });
        }

        private IActorRef GetChildActor(string id, User user)
        {
            if (_children.ContainsKey(id))
                return _children[id];

            var childActor = Context.ActorOf(
                Props.Create(() => new UserActor(id, user)), $"User-{id}");

            Context.Watch(childActor);
            _children = _children.Add(id, childActor);

            return childActor;
        }
    }
}