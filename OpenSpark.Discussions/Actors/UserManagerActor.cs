using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using OpenSpark.Shared.Commands;

namespace OpenSpark.Discussions.Actors
{
    // This parent actor needs to handle all requests coming in and forward them to the correct userActor
    public class UserManagerActor : ReceiveActor
    {
        private IImmutableDictionary<string, IActorRef> _children;

        public UserManagerActor()
        {
            _children = ImmutableDictionary<string, IActorRef>.Empty;

            Receive<ConnectUserCommand>(ConnectUser);

            Receive<DisconnectUserCommand>(command =>
            {
                RemoveUser(command.ConnectionId);
            });

            Receive<Terminated>(terminated =>
            {
                Context.Unwatch(terminated.ActorRef);
                _children = _children.Where(u => !u.Value.Equals(terminated.ActorRef)).ToImmutableDictionary();
            });

//            Receive<AddPostCommand>(command =>
//            {
//                _users[command.ConnectionId].Forward(command);
//            });

            Receive<FetchNewsFeedCommand>(command =>
            {
                if (!_children.ContainsKey(command.ConnectionId))
                {
                    Console.WriteLine("Error - Attempted to fetch news feed for non-existent user.");
                    return;
                };

                _children[command.ConnectionId].Forward(command);
            });
        }

        private void ConnectUser(ConnectUserCommand command)
        {
            RemoveUser(command.ConnectionId);

            var userActor = Context.ActorOf(
                Props.Create(() => new UserActor(command.ConnectionId, command.User)), $"User-{command.ConnectionId}");

            Context.Watch(userActor);
            _children = _children.Add(command.ConnectionId, userActor);
        }

        private void RemoveUser(string connectionId)
        {
            if (!_children.ContainsKey(connectionId)) return;

            var userActor = _children[connectionId];

            Context.Unwatch(userActor);
            userActor.GracefulStop(TimeSpan.FromSeconds(5));
            _children = _children.Where(u => !u.Value.Equals(userActor)).ToImmutableDictionary();
        }
    }
}
