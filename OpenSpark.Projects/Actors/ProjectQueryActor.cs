using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using System;

namespace OpenSpark.Projects.Actors
{
    public class ProjectQueryActor : ReceiveActor
    {
        public ProjectQueryActor()
        {
            SetReceiveTimeout(TimeSpan.FromMinutes(5));
            Receive<ProjectDetailsQuery>(HandleProjectDetailsQuery);
        }

        private void HandleProjectDetailsQuery(ProjectDetailsQuery query)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var ravenId = query.ProjectId.ConvertToRavenId<Project>();
            var project = session.Load<Project>(ravenId);

            if (project == null)
            {
                Sender.Tell(new PayloadEvent
                {
                    Callback = query.Callback,
                    ConnectionId = query.ConnectionId,
                    Errors = new [] {"This project could not be found. The owner may have removed it."},
                });

                return;
            }

            if (project.Visibility == VisibilityStatus.Private)
            {
                if (!project.TeamMembers.Contains(query.User.AuthUserId))
                {
                    Sender.Tell(new PayloadEvent
                    {
                        Callback = query.Callback,
                        ConnectionId = query.ConnectionId,
                        Errors = new[] {"You do not have permission to view this private project."},
                    });

                    return;
                }
            }

            Sender.Tell(new PayloadEvent
            {
                Callback = query.Callback,
                ConnectionId = query.ConnectionId,
                Payload = new ProjectDetailsViewModel
                {
                    ConnectedGroupId = project.ConnectedGroupId,
                    About            = project.About,
                    ProjectId        = query.ProjectId,
                    Name             = project.Name,
                    Visibility       = project.Visibility,
                    TotalSubscribers = project.Subscribers.Count,
                    Subscribed       = query.User.Projects.Contains(query.ProjectId),
                    LastUpdated      = project.LastUpdated.ConvertToHumanFriendlyFormat(),
                    TotalDownloads   = project.TotalDownloads,
                }
            });
        }
    }
}