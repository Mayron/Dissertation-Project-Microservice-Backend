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
        private readonly string _projectId;

        public ProjectActor(string projectId)
        {
            _projectId = projectId;

            Receive<ConnectProjectCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                var project = session.Query<Project>().SingleOrDefault(p => projectId == _projectId);

                if (project == null)
                    throw new ActorKilledException($"Failed to find project with id {command.ProjectId}");

                var isValid = IsVisibilityStatusValid(project.VisibilityStatus, command.GroupVisibilityStatus, command.TransactionId);
                if (!isValid) return;

                project.ConnectedGroupId = command.GroupId;
                session.SaveChanges();

                Sender.Tell(new ProjectConnectedEvent
                {
                    TransactionId = command.TransactionId,
                    Message = $"Project {project.Name} connected to group!",
                    ProjectId = _projectId
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }

        private bool IsVisibilityStatusValid(string projectVisibilityStatus, string groupVisibilityStatus, Guid transactionId)
        {
            if (projectVisibilityStatus != VisibilityStatus.Private &&
                groupVisibilityStatus == VisibilityStatus.Private)
            {
                var message = projectVisibilityStatus == VisibilityStatus.Unlisted ? "an unlisted" : "a public";

                Sender.Tell(new ProjectFailedToConnectEvent
                {
                    TransactionId = transactionId,
                    Message = $"Cannot connect a private group to {message} project.",
                    ProjectId = _projectId
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
                return false;
            }

            if (projectVisibilityStatus == VisibilityStatus.Public &&
                groupVisibilityStatus == VisibilityStatus.Unlisted)
            {
                Sender.Tell(new ProjectFailedToConnectEvent
                {
                    TransactionId = transactionId,
                    Message = "Cannot connect an unlisted group to a public project.",
                    ProjectId = _projectId
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
                return false;
            }

            return true;
        }
    }
}