using Akka.Actor;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.Shared;
using OpenSpark.Shared.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace OpenSpark.ApiGateway.Actors.PayloadAggregators
{
    public class GroupConnectionsListAggregator : BaseMultiPayloadAggregatorActor
    {
        public GroupConnectionsListAggregator(MultiQueryContext context, IActorRef callback) : base(context, callback)
        {
        }

        protected override object GetAggregatedPayload(List<object> payloads)
        {
            var project = payloads
                .Where(r => r is ProjectDetailsViewModel)
                .Cast<ProjectDetailsViewModel>()
                .Single();

            var groups = payloads
                .Where(s => s is List<UserGroupsViewModel>)
                .Cast<List<UserGroupsViewModel>>()
                .Single();

            var results = new List<ConnectionViewModel>();

            foreach (var group in groups)
            {
                var (canConnect, error) =
                    VisibilityHelper.CanProjectConnectToGroup(project.Visibility, group.Visibility);

                results.Add(new ConnectionViewModel
                {
                    Visibility = group.Visibility,
                    Id = group.Id,
                    Name = group.Name,
                    Available = canConnect,
                    NotAvailableMessage = error
                });
            }

            return results;
        }
    }
}