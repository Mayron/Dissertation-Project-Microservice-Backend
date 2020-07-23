using System;
using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using System.Collections.Generic;
using System.Linq;
using OpenSpark.Shared;
using OpenSpark.Shared.ViewModels;

namespace OpenSpark.Groups.Actors
{
    public class UserGroupsActor : ReceiveActor
    {
        public UserGroupsActor()
        {
            Receive<UserGroupsQuery>(query =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();
                List<Group> groups;

                if (query.OwnedGroups)
                {
                    groups = session.Query<Group>()
                        .Where(g => g.OwnerUserId == query.User.AuthUserId)
                        .OrderByDescending(g => g.Members.Count)
                        .Select(g => new Group
                        {
                            Id = g.Id,
                            Name = g.Name,
                            Visibility = g.Visibility
                        })
                        .ToList();
                }
                else if (query.Memberships)
                {
                    var userId = query.User.AuthUserId;

                    var members = session.Query<Member>()
                        .Include(m => m.GroupId)
                        .Where(m => m.AuthUserId == userId)
                        .OrderByDescending(m => m.Contribution)
                        .ToList();

                    groups = new List<Group>(members.Count);

                    foreach (var member in members)
                    {
                        // this will not require querying the server as we included it.
                        // We do not want groups we own as they go into their own "Groups" section.
                        var membership = session.Query<Group>()
                            .Select(g => new Group
                            {
                                Id = g.Id,
                                Name = g.Name
                            })
                            .FirstOrDefault(g => g.Id == member.GroupId && g.OwnerUserId != member.Id);

                        groups.Add(membership);
                    }
                }
                else
                {
                    throw new ActorKilledException("Invalid query request");
                }

                Sender.Tell(new PayloadEvent(query)
                {
                    Payload = groups.Select(g => new UserGroupsViewModel
                    {
                        Name = g.Name,
                        Id = g.Id.ConvertToEntityId(),
                        Visibility = g.Visibility
                    }).ToList()
                });

                Context.Stop(Self);
            });
        }
    }
}