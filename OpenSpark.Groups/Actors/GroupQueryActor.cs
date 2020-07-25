using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Groups.Indexes;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Routing;
using OpenSpark.Shared;
using Raven.Client.Documents;
using Group = OpenSpark.Domain.Group;

namespace OpenSpark.Groups.Actors
{
    public class GroupQueryActor : ReceiveActor
    {
        private readonly GroupRepository _groupRepository;

        public static Props Props { get; } = Props.Create<GroupQueryActor>()
            .WithRouter(new RoundRobinPool(5,
                new DefaultResizer(1, 10)));

        public GroupQueryActor()
        {
            _groupRepository = new GroupRepository();
            Receive<GroupDetailsQuery>(HandleGroupDetailsQuery);
            Receive<UserGroupsQuery>(HandleUserGroupsQuery);
        }

        private void HandleGroupDetailsQuery(GroupDetailsQuery query)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var ravenGroupId = query.GroupId.ConvertToRavenId<Group>();
            var result = session.Query<GetBasicGroupDetails.Result, GetBasicGroupDetails>()
                .SingleOrDefault(g => g.GroupId == ravenGroupId);

            if (result == null)
            {
                Sender.Tell(new PayloadEvent(query)
                {
                    Errors = new [] {"This group could not be found. The owner may have removed it."},
                });

                return;
            }

            if (!IsUserRestrictedFromQuerying(query, ravenGroupId, result.Visibility)) return;

            var categoryId = result.CategoryId;
            var category = session.Load<Category>(categoryId);

            if (category == null)
                Console.WriteLine($"Failed to find group category with id: {categoryId}");

            if (query.RetrieveGroupNameOnly)
            {
                Sender.Tell(new PayloadEvent(query) { 
                    Payload = new NamedEntityViewModel
                    {
                        Id = query.GroupId,
                        Name = result.Name
                    }
                });

                return;
            }

            var isMember = query.User != null && query.User.Groups.Contains(query.GroupId);

            Sender.Tell(new PayloadEvent(query)
            {
                Payload = new GroupDetailsViewModel
                {
                    About = result.About,
                    CategoryName = category?.Name ?? "Unknown",
                    GroupId = result.GroupId.ConvertToEntityId(),
                    Name = result.Name,
                    Visibility = result.Visibility,
                    TotalMembers = result.TotalMembers,
                    IsMember = isMember,
                }
            });
        }

        private void HandleUserGroupsQuery(UserGroupsQuery query)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();
            List<Group> groups = new List<Group>();

            if (query.OwnedGroups)
            {
                try
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

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
        }

        private bool IsUserRestrictedFromQuerying(IQuery query, string ravenGroupId, string groupVisibility)
        {
            if (groupVisibility != VisibilityStatus.Private) return true;

            var member = _groupRepository.GetGroupMemberByAuthUserId(query.User.AuthUserId, ravenGroupId);
            if (member != null) return true;

            Sender.Tell(new PayloadEvent(query)
            {
                Errors = new[] { "You do not have permission to view this private group." },
            });

            return false;
        }
    }
}