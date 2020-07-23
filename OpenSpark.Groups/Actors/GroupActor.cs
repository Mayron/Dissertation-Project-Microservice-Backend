using Akka.Actor;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Queries;
using System;

namespace OpenSpark.Groups.Actors
{
    public class GroupActor : ReceiveActor
    {
        public GroupActor()
        {
            SetReceiveTimeout(TimeSpan.FromMinutes(30));

            Receive<DeleteGroupCommand>(command =>
            {
                var verifyActor = Context.ActorOf(
                    Props.Create<DeleteGroupActor>(), $"DeleteGroup-{command.GroupId}");

                verifyActor.Forward(command);

                Context.Stop(Self);
            });

            Receive<GroupDetailsQuery>(query =>
            {
                var queryActor = Context.ActorOf(
                    Props.Create(() => new GroupQueryActor(new GroupRepository())),
                    $"GroupQuery-{query.ConnectionId}");

                queryActor.Forward(query);
            });


            Receive<GroupProjectsQuery>(query =>
            {
                var queryActor = Context.ActorOf(
                    Props.Create(() => new GroupQueryActor(new GroupRepository())),
                    $"GroupQuery-{query.ConnectionId}");

                queryActor.Forward(query);
            });
        }
    }
}