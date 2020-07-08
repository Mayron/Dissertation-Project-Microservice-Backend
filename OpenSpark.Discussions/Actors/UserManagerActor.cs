using System;
using System.Collections.Generic;
using Akka.Actor;
using OpenSpark.Discussions.Commands;
using OpenSpark.Domain;

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
                _users[command.ConnectionId].GracefulStop(TimeSpan.FromSeconds(5));
                _users.Remove(command.ConnectionId);
            });

//            Receive<AddPostCommand>(command =>
//            {
//                _users[command.ConnectionId].Forward(command);
//            });

            Receive<FetchNewsFeedCommand>(command =>
            {
                _users[command.ConnectionId].Forward(command);
            });
        }

        private void ConnectUser(ConnectUserCommand command)
        {
            if (_users.ContainsKey(command.ConnectionId)) return;

            var userActor = Context.ActorOf(
                Props.Create(() => new UserActor(command.ConnectionId, command.User)), command.ConnectionId);

            _users.Add(command.ConnectionId, userActor);
        }
    }
}
