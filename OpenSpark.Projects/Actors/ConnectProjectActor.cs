using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Events.Sagas;
using System;

namespace OpenSpark.Projects.Actors
{
    public class ConnectProjectActor : ReceiveActor
    {
        public ConnectProjectActor()
        {
            Receive<ConnectProjectCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                var project = session.Load<Project>($"project/{command.ProjectId}");

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

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }

        private bool IsVisibilityStatusValid(ConnectProjectCommand command, string projectVisibility)
        {
            if (projectVisibility != VisibilityStatus.Private &&
                command.GroupVisibility == VisibilityStatus.Private)
            {
                var message = projectVisibility == VisibilityStatus.Unlisted ? "an unlisted" : "a public";

                Sender.Tell(new ProjectFailedToConnectEvent
                {
                    TransactionId = command.TransactionId,
                    Message = $"Cannot connect a private group to {message} project.",
                    ProjectId = command.ProjectId
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
                return false;
            }

            if (projectVisibility == VisibilityStatus.Public &&
                command.GroupVisibility == VisibilityStatus.Unlisted)
            {
                Sender.Tell(new ProjectFailedToConnectEvent
                {
                    TransactionId = command.TransactionId,
                    Message = "Cannot connect an unlisted group to a public project.",
                    ProjectId = command.ProjectId
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
                return false;
            }

            return true;
        }
    }
}