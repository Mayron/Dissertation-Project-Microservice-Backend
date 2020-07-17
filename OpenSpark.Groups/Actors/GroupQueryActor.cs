using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Groups.Indexes;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using System;
using System.Linq;

namespace OpenSpark.Groups.Actors
{
    public class GroupQueryActor : ReceiveActor
    {
        private readonly GroupRepository _groupRepository;

        public GroupQueryActor(GroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
            Receive<BasicGroupDetailsQuery>(HandleBasicGroupDetailsQuery);
        }

        private void HandleBasicGroupDetailsQuery(BasicGroupDetailsQuery query)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var groupId = query.GroupId;
            var result = session.Query<GetBasicGroupDetails.Result, GetBasicGroupDetails>()
                .SingleOrDefault(g => g.GroupId == groupId);

            if (result == null)
            {
                Sender.Tell(new PayloadEvent
                {
                    Callback = query.Callback,
                    ConnectionId = query.ConnectionId,
                    Error = "This group could not be found. The owner may have removed it.",
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
                return;
            }

            if (result.Visibility == VisibilityStatus.Private)
            {
                var member = _groupRepository.GetGroupMember(query.User.AuthUserId, query.GroupId);
                if (member == null)
                {
                    Sender.Tell(new PayloadEvent
                    {
                        Callback = query.Callback,
                        ConnectionId = query.ConnectionId,
                        Error = "You do not have permission to view this private group.",
                    });

                    Self.GracefulStop(TimeSpan.FromSeconds(5));
                    return;
                }
            }

            var categoryId = result.CategoryId;
            var category = session.Query<GroupCategory>().SingleOrDefault(c => c.Id == categoryId);

            if (category == null)
                Console.WriteLine($"Failed to find group category with id: {categoryId}");

            Sender.Tell(new PayloadEvent
            {
                Callback = query.Callback,
                ConnectionId = query.ConnectionId,
                Payload = new BasicGroupDetailsViewModel
                {
                    About = result.About,
                    CategoryName = category?.Name ?? "Unknown",
                    GroupId = result.GroupId,
                    Name = result.Name,
                    Visibility = result.Visibility
                }
            });

            Self.GracefulStop(TimeSpan.FromSeconds(5));
        }
    }
}