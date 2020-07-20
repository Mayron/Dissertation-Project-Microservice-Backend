using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Events.Sagas;
using System;
using OpenSpark.Shared;

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

                var ravenId = command.ProjectId.ConvertToRavenId<Project>();
                var project = session.Load<Project>(ravenId);

                if (project == null)
                    throw new ActorKilledException($"Failed to find project with id {command.ProjectId}");

                var isValid = IsVisibilityStatusValid(command, project.Visibility);
                if (!isValid) return;

                project.ConnectedGroupId = command.GroupId;
                session.SaveChanges();

                Sender.Tell(new ProjectConnectedEvent
                {
                    TransactionId = command.TransactionId,
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
                TransactionId = command.TransactionId,
                Message = error,
                ProjectId = command.ProjectId
            });

            Context.Stop(Self);
            return false;
        }
    }
}