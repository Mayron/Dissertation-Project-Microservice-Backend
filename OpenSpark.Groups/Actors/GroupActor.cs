using Akka.Actor;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Commands.Posts;

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