using Akka.Actor;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Events.ConnectProject;
using System;
using OpenSpark.Projects.Domain;

namespace OpenSpark.Projects.Actors
{
    public class ConnectProjectActor : ReceiveActor
    {
        public ConnectProjectActor()
        {
            SetReceiveTimeout(TimeSpan.FromMinutes(5));

            Receive<ConnectProjectCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                var ravenProjectId = command.ProjectId.ConvertToRavenId<Project>();
                var project = session.Load<Project>(ravenProjectId);

                if (project == null)
                    throw new ActorKilledException($"Failed to find project with id {command.ProjectId}");

                var isValid = IsVisibilityStatusValid(command, project.Visibility);
                if (!isValid) return;

                var ravenGroupId = command.GroupId;

                project.ConnectedGroupId = ravenGroupId;
                project.LinkedGroups.Add(ravenGroupId); // displayed on group page
                session.SaveChanges();

                Sender.Tell(new ProjectConnectedEvent
                {
                    Message = $"Project {project.Name} connected to group!",
                    ProjectId = command.ProjectId
                });
            });
        }

        private bool IsVisibilityStatusValid(ConnectProjectCommand command, string projectVisibility)
        {
            var (canConnect, error) =
                VisibilityHelper.CanProjectConnectToGroup(projectVisibility, command.GroupVisibility);

            if (canConnect) return true;

            Sender.Tell(new ProjectFailedToConnectEvent
            {
                Message = error,
                ProjectId = command.ProjectId
            });

            Context.Stop(Self);
            return false;
        }
    }
}