using System.Linq;
using Akka.Actor;
using Akka.Routing;
using OpenSpark.Shared;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using OpenSpark.Teams.Domain;

namespace OpenSpark.Teams.Actors
{
    internal class TeamQueryActor : ReceiveActor
    {
        public static Props Props { get; } = Props.Create<TeamQueryActor>()
            .WithRouter(new RoundRobinPool(2,
                new DefaultResizer(1, 5)));

        public TeamQueryActor()
        {
            Receive<TeamsQuery>(query =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                var teams = session.Query<Team>()
                    .Where(t => t.ProjectId == query.ProjectId)
                    .ToList();

                var viewModel = teams.Select(t => new TeamViewModel
                {
                    TeamId = t.Id.ConvertToClientId(),
                    Name = t.Name,
                    Description = t.Description,
                    TotalMembers = t.Members.Count,
                    Color = t.Color
                }).ToList();

                Sender.Tell(new PayloadEvent(query)
                {
                    Payload = viewModel
                });
            });
        }
    }
}