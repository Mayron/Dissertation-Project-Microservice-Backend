using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenSpark.Shared;
using OpenSpark.Shared.ViewModels;

namespace OpenSpark.Projects.Actors
{
    public class UserProjectsActor : ReceiveActor
    {
        public UserProjectsActor()
        {
            Receive<UserProjectsQuery>(query =>
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

                Sender.Tell(new PayloadEvent
                {
                    ConnectionId = query.ConnectionId,
                    Callback = query.Callback,
                    Payload = projects.Select(p => new UserGroupsViewModel
                    {
                        Name = p.Name,
                        Id = p.Id.ConvertToEntityId()
                    }).ToList()
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }
    }
}