using System;
using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using System.Collections.Generic;
using System.Linq;

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
                            .FirstOrDefault(g => g.Id == member.GroupId && g.OwnerUserId != member.Id);

                        groups.Add(membership);
                    }
                }
                else
                {
                    throw new ActorKilledException("Invalid query request");
                }

                Sender.Tell(new PayloadEvent
                {
                    ConnectionId = query.ConnectionId,
                    Callback = query.Callback,
                    Payload = groups
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }
    }
}