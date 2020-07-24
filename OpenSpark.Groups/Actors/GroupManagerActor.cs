using Akka.Actor;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Commands.Posts;
using OpenSpark.Shared.Queries;
using System.Collections.Immutable;
using System.Linq;

namespace OpenSpark.Groups.Actors
{
    public class GroupManagerActor : ReceiveActor
    {
        private IImmutableDictionary<string, IActorRef> _children;
        private IActorRef _categoriesActor;
        private readonly IActorRef _searchPool = Context.ActorOf(SearchGroupsActor.Props, "SearchQueryPool");
        private readonly IActorRef _createGroupPool = Context.ActorOf(CreateGroupActor.Props, "CreateGroupPool");
        private readonly IActorRef _verifyPostPool = Context.ActorOf(VerifyPostActor.Props, "VerifyPostPool");
        private readonly IActorRef _groupQueryPool = Context.ActorOf(GroupQueryActor.Props, "GroupQueryPool");

        public GroupManagerActor()
        {
            _children = ImmutableDictionary<string, IActorRef>.Empty;
            
            Receive<DeleteGroupCommand>(query => ForwardByGroupId(query.GroupId, query));
            Receive<CategoriesQuery>(query =>
            {
                if (_categoriesActor == null)
                     _categoriesActor = Context.Watch(Context.ActorOf(Props.Create<CategoriesActor>(), "Categories"));

                _categoriesActor.Forward(query);
            });

            // Pools
            Receive<GroupDetailsQuery>(query => _groupQueryPool.Forward(query));
            Receive<UserGroupsQuery>(query => _groupQueryPool.Forward(query));
            Receive<SearchGroupsQuery>(query => _searchPool.Forward(query));
            Receive<CreateGroupCommand>(command => _createGroupPool.Forward(command));
            Receive<VerifyPostRequestCommand>(command => _verifyPostPool.Forward(command));

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);

                if (_categoriesActor != null && _categoriesActor.Equals(terminated.ActorRef))
                {
                    _categoriesActor = null;
                    return;
                }

                if (!_children.Any(u => u.Value.Equals(terminated.ActorRef))) return;
                _children = _children.Where(u => !u.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
            });
        }

        private void ForwardByGroupId(string groupId, IMessage message)
        {
            var actorRef = GetChildActor(groupId);
            actorRef.Forward(message);
        }

        private IActorRef GetChildActor(string id)
        {
            if (_children.ContainsKey(id))
                return _children[id];

            var childActor = Context.ActorOf(Props.Create<GroupActor>(), $"Group-{id}");

            Context.Watch(childActor);
            _children = _children.Add(id, childActor);

            return childActor;
        }
    }
}