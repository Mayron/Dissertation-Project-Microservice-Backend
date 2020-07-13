using System;
using System.Collections.Generic;
using Akka.Actor;
using OpenSpark.Shared.Commands;

namespace OpenSpark.Discussions.Actors
{
    // This parent actor needs to handle all requests coming in and forward them to the correct userActor
    public class UserManagerActor : ReceiveActor
    {
        private readonly Dictionary<string, IActorRef> _users;

        public UserManagerActor()
        {
            _users = new Dictionary<string, IActorRef>();

            Receive<ConnectUserCommand>(ConnectUser);

            Receive<DisconnectUserCommand>(command =>
            {
                RemoveUser(command.ConnectionId);
            });

//            Receive<AddPostCommand>(command =>
//            {
//                _users[command.ConnectionId].Forward(command);
//            });

            Receive<FetchNewsFeedCommand>(command =>
            {
                if (!_users.ContainsKey(command.ConnectionId))
                {
                    Console.WriteLine("Error - Attempted to fetch news feed for non-existent user.");
                    return;
                };

                _users[command.ConnectionId].Forward(command);
            });
        }

        private void ConnectUser(ConnectUserCommand command)
        {
            RemoveUser(command.ConnectionId);

            var userActor = Context.ActorOf(
                Props.Create(() => new UserActor(command.ConnectionId, command.User)), command.ConnectionId);

            _users.Add(command.ConnectionId, userActor);
        }

        private void RemoveUser(string connectionId)
        {
            if (!_users.ContainsKey(connectionId)) return;

            _users[connectionId].GracefulStop(TimeSpan.FromSeconds(5));
            _users.Remove(connectionId);
        }
    }
}
