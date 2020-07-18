using System;
using Akka.Actor;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Commands.Posts;
using OpenSpark.Shared.Queries;

namespace OpenSpark.Groups.Actors
{
    public class GroupActor : ReceiveActor
    {
        public GroupActor(string groupId)
        {
            SetReceiveTimeout(TimeSpan.FromMinutes(30));

            Receive<VerifyUserPostRequestCommand>(command =>
            {
                var verifyActor = Context.ActorOf(
                    Props.Create(() => new VerifyUserPostActor(groupId, new GroupRepository())), 
                    $"VerifyUserPost-{command.TransactionId}");

                verifyActor.Forward(command);
            });

            Receive<DeleteGroupCommand>(command =>
            {
                var verifyActor = Context.ActorOf(
                    Props.Create<DeleteGroupActor>(), $"DeleteGroup-{command.GroupId}");

                verifyActor.Forward(command);

                Self.GracefulStop(TimeSpan.FromSeconds(5));
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