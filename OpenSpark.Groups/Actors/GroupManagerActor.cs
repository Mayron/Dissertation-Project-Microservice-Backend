﻿using Akka.Actor;
using System.Collections.Immutable;
using System.Linq;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Commands.Posts;
using OpenSpark.Shared.Events.Sagas.CreateGroup;
using OpenSpark.Shared.Queries;

namespace OpenSpark.Groups.Actors
{
    public class GroupManagerActor : ReceiveActor
    {
        private IImmutableDictionary<string, IActorRef> _children;
        private IActorRef _categoriesActor;

        public GroupManagerActor()
        {
            _children = ImmutableDictionary<string, IActorRef>.Empty;

            Receive<VerifyUserPostRequestCommand>(command => ForwardByGroupId(command.GroupId, command));
            Receive<BasicGroupDetailsQuery>(query => ForwardByGroupId(query.GroupId, query));
            Receive<DeleteGroupCommand>(query => ForwardByGroupId(query.GroupId, query));
            Receive<CategoriesQuery>(query =>
            {
                if (_categoriesActor == null)
                {
                    _categoriesActor = Context.ActorOf(Props.Create<CategoriesActor>(), "Categories");
                    Context.Watch(_categoriesActor);
                }

                _categoriesActor.Forward(query);
            });

            Receive<UserGroupsQuery>(query =>
            {
                var actorRef = Context.ActorOf(Props.Create<UserGroupsActor>());
                actorRef.Forward(query);
            });

            Receive<CreateGroupCommand>(command =>
            {
                var actorRef = Context.ActorOf(
                    Props.Create<CreateGroupActor>(), $"CreateGroup-{command.TransactionId}");

                actorRef.Forward(command);
            });

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);
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

            var childActor = Context.ActorOf(
                Props.Create(() => new GroupActor(id)), $"Group-{id}");

            Context.Watch(childActor);
            _children = _children.Add(id, childActor);

            return childActor;
        }
    }
}