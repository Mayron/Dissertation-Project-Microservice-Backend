using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Events.Sagas;
using System;
using System.Linq;

namespace OpenSpark.Projects.Actors
{
    public class ProjectActor : ReceiveActor
    {
        public ProjectActor()
        {
            SetReceiveTimeout(TimeSpan.FromMinutes(30));

            Receive<ConnectProjectCommand>(command =>
            {
                var actorRef = Context.ActorOf(
                    Props.Create<ConnectProjectActor>(), $"ConnectProject-{command.ProjectId}");

                actorRef.Forward(command);
            });

            Receive<DeleteProjectCommand>(command =>
            {
                var actorRef = Context.ActorOf(
                    Props.Create<DeleteProjectActor>(), $"DeleteProject-{command.ProjectId}");

                actorRef.Forward(command);

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }
    }
}