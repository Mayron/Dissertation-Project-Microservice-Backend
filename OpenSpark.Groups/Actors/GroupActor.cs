using Akka.Actor;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Queries;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OpenSpark.Shared.Events.Payloads;

namespace OpenSpark.Groups.Actors
{
    public class GroupActor : ReceiveActor
    {
        // Handle less common commands or messages that rely on group state
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
        }
    }
}