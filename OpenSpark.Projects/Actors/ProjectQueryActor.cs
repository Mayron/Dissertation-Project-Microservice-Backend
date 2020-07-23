using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Akka.Routing;

namespace OpenSpark.Projects.Actors
{
    public class ProjectQueryActor : ReceiveActor
    {
        public static Props Props { get; } = Props.Create<ProjectQueryActor>()
            .WithRouter(new RoundRobinPool(2,
                new DefaultResizer(1, 5)));

        public ProjectQueryActor()
        {
            Receive<ProjectDetailsQuery>(HandleProjectDetailsQuery);
            Receive<UserProjectsQuery>(HandleUserProjectsQuery);
        }

        private void HandleUserProjectsQuery(UserProjectsQuery query)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            List<Project> projects;
            var userId = query.User.AuthUserId;

            if (query.OwnedProjects)
            {
                projects = session.Query<Project>()
                    .Where(g => g.OwnerUserId == userId)
                    .OrderByDescending(g => g.Subscribers.Count)
                    .Select(p => new Project
                    {
                        Id = p.Id,
                        Name = p.Name
                    })
                    .ToList();
            }
            else if (query.Subscriptions)
            {
                // We do not want projects we own as they go into their own "Projects" section.
                projects = session.Query<Project>()
                    .Where(p => p.OwnerUserId != userId && p.Subscribers.Contains(userId))
                    .OrderByDescending(g => g.Subscribers.Count)
                    .Select(p => new Project
                    {
                        Id = p.Id,
                        Name = p.Name
                    })
                    .ToList();
            }
            else
            {
                throw new ActorKilledException("Invalid query request");
            }

            Sender.Tell(new PayloadEvent(query)
            {
                Payload = projects.Select(p => new UserGroupsViewModel
                {
                    Name = p.Name,
                    Id = p.Id.ConvertToEntityId()
                }).ToList()
            });
        }

        // Used when the user lands on a Projects page
        private void HandleProjectDetailsQuery(ProjectDetailsQuery query)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var ravenId = query.ProjectId.ConvertToRavenId<Project>();
            var project = session.Load<Project>(ravenId);

            if (project == null)
            {
                Sender.Tell(new PayloadEvent(query)
                {
                    Errors = new[] { "This project could not be found. The owner may have removed it." },
                });

                return;
            }

            if (project.Visibility == VisibilityStatus.Private)
            {
                if (!project.TeamMembers.Contains(query.User.AuthUserId))
                {
                    Sender.Tell(new PayloadEvent(query)
                    {
                        Errors = new[] { "You do not have permission to view this private project." },
                    });

                    return;
                }
            }

            if (query.RetrieveProjectNameOnly)
            {
                Sender.Tell(new PayloadEvent(query) { Payload = project.Name });
                return;
            }

            Sender.Tell(new PayloadEvent(query)
            {
                Payload = new ProjectDetailsViewModel
                {
                    ConnectedGroupId = project.ConnectedGroupId,
                    About = project.About,
                    ProjectId = query.ProjectId,
                    Name = project.Name,
                    Visibility = project.Visibility,
                    TotalSubscribers = project.Subscribers.Count,
                    Subscribed = query.User.Projects.Contains(query.ProjectId),
                    LastUpdated = project.LastUpdated.ConvertToHumanFriendlyFormat(),
                    TotalDownloads = project.TotalDownloads,
                }
            });
        }
    }
}