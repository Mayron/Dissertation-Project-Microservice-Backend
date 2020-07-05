using System.Collections.Generic;
using Akka.Actor;
using OpenSpark.ActorModel.Commands;
using OpenSpark.Domain;

namespace OpenSpark.ActorModel.Actors
{
    // This parent actor needs to handle all requests coming in and forward them to the correct userActor
    public class UserManagerActor : ReceiveActor
    {
        private readonly Dictionary<User, IActorRef> _users;

        public UserManagerActor()
        {
            _users = new Dictionary<User, IActorRef>();

            Receive<ConnectUserCommand>(ConnectUser);

            Receive<AddPostCommand>(command =>
            {
                _users[command.User].Forward(command);
            });
        }

        private void ConnectUser(ConnectUserCommand command)
        {
            if (_users.ContainsKey(command.User)) return;

            var userActorName = command.User.UserId;
            var userActor = Context.ActorOf(
                Props.Create(() => new UserActor(command.User)), userActorName);

            _users.Add(command.User, userActor);
        }
    }
}
