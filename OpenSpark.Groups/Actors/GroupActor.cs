using Akka.Actor;
using OpenSpark.Shared.Commands.Sagas.CreateGroup;
using OpenSpark.Shared.Commands.Sagas.CreatePost;

namespace OpenSpark.Groups.Actors
{
    public class GroupActor : ReceiveActor
    {
        public GroupActor(string id)
        {
            Receive<VerifyUserPostRequestCommand>(command =>
            {
                var verifyActor = Context.ActorOf(
                    Props.Create(() => new VerifyUserPostActor(id)), $"VerifyUserPost-{command.TransactionId}");

                verifyActor.Forward(command);
            });

            Receive<CreateGroupCommand>(command =>
            {
                var verifyActor = Context.ActorOf(
                    Props.Create<CreateGroupActor>(), $"CreateGroup-{command.TransactionId}");

                verifyActor.Forward(command);
            });
        }
    }
}