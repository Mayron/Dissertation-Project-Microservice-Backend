using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Groups.Indexes;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using System;
using System.Linq;
using Akka.Routing;
using OpenSpark.Shared;
using Group = OpenSpark.Domain.Group;

namespace OpenSpark.Groups.Actors
{
    public class GroupQueryActor : ReceiveActor
    {
        private readonly GroupRepository _groupRepository;

        public GroupQueryActor()
        {
            _groupRepository = new GroupRepository();
            Receive<GroupDetailsQuery>(HandleGroupDetailsQuery);
            Receive<GroupProjectsQuery>(HandleGroupProjectsQuery);
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
                Sender.Tell(new PayloadEvent(query) { Payload = result.Name });
                return;
            }

            Sender.Tell(new PayloadEvent(query)
            {
                Payload = new GroupDetailsViewModel
                {
                    About = result.About,
                    CategoryName = category?.Name ?? "Unknown",
                    GroupId = result.GroupId.ConvertToEntityId(),
                    Name = result.Name,
                    Visibility = result.Visibility,
                    TotalMembers = result.TotalMembers
                }
            });
        }
        
        private void HandleGroupProjectsQuery(GroupProjectsQuery query)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var ravenGroupId = query.GroupId.ConvertToRavenId<Group>();
            var group = session.Load<Group>(ravenGroupId);

            if (group == null)
            {
                Sender.Tell(new PayloadEvent(query)
                {
                    Errors = new[] { "This group could not be found. The owner may have removed it." },
                });

                return;
            }

            if (!IsUserRestrictedFromQuerying(query, ravenGroupId, group.Visibility)) return;

            Sender.Tell(new PayloadEvent(query)
            {
                Payload = group.ListedProjects
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

        public static Props Props { get; } = Props.Create<GroupQueryActor>()
            .WithRouter(new RoundRobinPool(5,
                new DefaultResizer(1, 10)));
    }
}