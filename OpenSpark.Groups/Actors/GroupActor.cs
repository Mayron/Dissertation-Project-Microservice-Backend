using Akka.Actor;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Commands.Posts;
using OpenSpark.Shared.Queries;

namespace OpenSpark.Groups.Actors
{
    public class GroupActor : ReceiveActor
    {
        public GroupActor(string id)
        {
            Receive<VerifyUserPostRequestCommand>(command =>
            {
                var verifyActor = Context.ActorOf(
                    Props.Create(() => new VerifyUserPostActor(id, new GroupRepository())), 
                    $"VerifyUserPost-{command.TransactionId}");

                verifyActor.Forward(command);
            });

            Receive<CreateGroupCommand>(command =>
            {
                var verifyActor = Context.ActorOf(
                    Props.Create<CreateGroupActor>(), $"CreateGroup-{command.TransactionId}");

                verifyActor.Forward(command);
            });

            Receive<BasicGroupDetailsQuery>(query =>
            {
                var queryActor = Context.ActorOf(
                    Props.Create(() => new GroupQueryActor(new GroupRepository())), 
                    $"GroupQuery-{query.ConnectionId}");

                queryActor.Forward(query);
            });
        }
    }
}