using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Sagas.CreatePost;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Events.Sagas;
using System;
using System.Linq;
using OpenSpark.Shared.Commands.Sagas.CreateGroup;

namespace OpenSpark.Groups.Actors
{
    public class GroupActor : ReceiveActor
    {
        private readonly string _groupId;
        private readonly Lazy<Group> _state;

        private Group Group => _state.Value;

        public GroupActor(string groupId)
        {
            _groupId = groupId;
            _state = new Lazy<Group>(FetchGroupDocument);

            Receive<VerifyUserPostRequestCommand>(command =>
            {
                var verifyActor = Context.ActorOf(
                    Props.Create(() => new VerifyUserPostActor(Group)), $"VerifyUserPost-{command.TransactionId}");

                verifyActor.Forward(command);
            });

            Receive<CreateGroupCommand>(command =>
            {
                // TODO
            });
        }

        public Group FetchGroupDocument()
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var group = session.Query<Group>().SingleOrDefault(g => g.GroupId == _groupId);

            if (group != null) return group;

            var message = $"Failed to retrieve group: {_groupId}";
            Sender.Tell(new ErrorEvent { Message = message });
            throw new ActorKilledException(message);
        }
    }
}