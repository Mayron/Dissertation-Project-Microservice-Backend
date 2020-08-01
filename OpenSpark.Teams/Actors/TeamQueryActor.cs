using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Routing;
using OpenSpark.Shared;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.Queries.Teams;
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

            Receive<TeamMembersQuery>(query =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();
                var ravenTeamId = query.TeamId.ConvertToRavenId<Team>();
                var members = session.Query<Member>().Where(m => m.TeamId == ravenTeamId).ToList();

                var payload = new List<TeamMemberViewModel>(members.Count);

                foreach (var member in members)
                {
                    var teamNames = session.Query<Team>()
                        .Where(t => t.Members.Contains(member.Id))
                        .Select(t => t.Name)
                        .ToList();

                    var memberViewModel = new TeamMemberViewModel
                    {
                        UserId = member.Id.ConvertToClientId(),
                        Contributions = member.Contributions,
                        Teams = string.Join(", ", teamNames)
                    };

                    payload.Add(memberViewModel);
                }

                Sender.Tell(new PayloadEvent(query)
                {
                    Payload = payload
                });
            });

            Receive<TeamPermissionsQuery>(query =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();
                var ravenTeamId = query.TeamId.ConvertToRavenId<Team>();
                var team = session.Load<Team>(ravenTeamId);

                Sender.Tell(new PayloadEvent(query)
                {
                    Payload = team.Permissions.ToList()
                });
            });
        }
    }
}