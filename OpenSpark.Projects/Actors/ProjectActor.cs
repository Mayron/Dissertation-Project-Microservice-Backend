using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Events.Sagas;
using Raven.Client.Documents;
using System;
using System.Threading.Tasks;
using OpenSpark.Shared.Commands.Projects;

namespace OpenSpark.Projects.Actors
{
    public class ProjectActor : ReceiveActor
    {
        private readonly string _projectId;

        public ProjectActor(string projectId)
        {
            _projectId = projectId;

            ReceiveAsync<ConnectProjectCommand>(async command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenAsyncSession();

                var project = await session.Query<Project>().SingleOrDefaultAsync(p => projectId == _projectId);

                if (project == null)
                    throw new ActorKilledException($"Failed to find project with id {command.ProjectId}");

                var isValid = await IsVisibilityStatusValid(project.VisibilityStatus, command.GroupVisibilityStatus, command.TransactionId);
                if (!isValid) return;

                project.ConnectedGroupId = command.GroupId;
                await session.SaveChangesAsync();

                Sender.Tell(new ProjectConnectedEvent
                {
                    TransactionId = command.TransactionId,
                    Message = $"Project {project.Name} connected to group!",
                    ProjectId = _projectId
                });

                await Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }

        private async Task<bool> IsVisibilityStatusValid(string projectVisibilityStatus, string groupVisibilityStatus, Guid transactionId)
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

                await Self.GracefulStop(TimeSpan.FromSeconds(5));
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

                await Self.GracefulStop(TimeSpan.FromSeconds(5));
                return false;
            }

            return true;
        }
    }
}