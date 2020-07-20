using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Groups.Indexes;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using System;
using System.Linq;
using OpenSpark.Shared;

namespace OpenSpark.Groups.Actors
{
    public class GroupQueryActor : ReceiveActor
    {
        private readonly GroupRepository _groupRepository;

        public GroupQueryActor(GroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
            Receive<GroupDetailsQuery>(HandleGroupDetailsQuery);
        }

        private void HandleGroupDetailsQuery(GroupDetailsQuery query)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var groupId = query.GroupId.ConvertToRavenId<Group>();
            var result = session.Query<GetBasicGroupDetails.Result, GetBasicGroupDetails>()
                .SingleOrDefault(g => g.GroupId == groupId);

            if (result == null)
            {
                Sender.Tell(new PayloadEvent
                {
                    Callback = query.Callback,
                    ConnectionId = query.ConnectionId,
                    Errors = new [] {"This group could not be found. The owner may have removed it."},
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
                return;
            }

            if (result.Visibility == VisibilityStatus.Private)
            {
                var member = _groupRepository.GetGroupMemberByAuthUserId(query.User.AuthUserId, query.GroupId);
                if (member == null)
                {
                    Sender.Tell(new PayloadEvent
                    {
                        Callback = query.Callback,
                        ConnectionId = query.ConnectionId,
                        Errors = new [] {"You do not have permission to view this private group."},
                    });

                    Self.GracefulStop(TimeSpan.FromSeconds(5));
                    return;
                }
            }

            var categoryId = result.CategoryId;
            var category = session.Load<Category>(categoryId);

            if (category == null)
                Console.WriteLine($"Failed to find group category with id: {categoryId}");

            Sender.Tell(new PayloadEvent
            {
                Callback = query.Callback,
                ConnectionId = query.ConnectionId,
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

            Self.GracefulStop(TimeSpan.FromSeconds(5));
        }
    }
}