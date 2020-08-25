using Akka.Actor;
using Akka.Routing;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Teams;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.RavenDb;
using OpenSpark.Teams.Domain;
using System.Collections.Generic;
using System.Linq;

namespace OpenSpark.Teams.Actors
{
    internal class TeamCommandActor : ReceiveActor
    {
        public static Props Props { get; } = Props.Create<TeamCommandActor>()
            .WithRouter(new RoundRobinPool(1,
                new DefaultResizer(1, 10)));

        public TeamCommandActor()
        {
            Receive<CreateTeamCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();
                var ravenMemberId = session.GenerateRavenId<Member>();

                var team = new Team
                {
                    Id = session.GenerateRavenId<Team>(),
                    Name = command.TeamName,
                    ProjectId = command.ProjectId,
                    Permissions = TeamPermissionsHelper.GetDefaultNewTeamPermissions(),
                    Members = new List<string> { ravenMemberId },
                    Description = command.Description,
                    Color = command.Color
                };

                var member = new Member
                {
                    Id = ravenMemberId,
                    TeamId = team.Id,
                    UserAuthId = command.User.AuthUserId
                };

                session.Store(team);
                session.Store(member);
                session.SaveChanges();

                Sender.Tell(new PayloadEvent(command)
                {
                    Payload = team.Id.ConvertToClientId()
                });
            });

            Receive<CreateDefaultTeamsCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                var adminsTeam = new Team
                {
                    Id = session.GenerateRavenId<Team>(),
                    Name = "Admins",
                    ProjectId = command.ProjectId,
                    Permissions = TeamPermissionsHelper.GetDefaultAdminPermissions(),
                    Members = new List<string> { session.GenerateRavenId<Member>() },
                    Description = "Members in this team have full access to all aspects of the project, including editing team permissions.",
                    Color = "#FF4F00"
                };

                var moderatorsTeam = new Team
                {
                    Id = session.GenerateRavenId<Team>(),
                    Name = "Moderators",
                    ProjectId = command.ProjectId,
                    Permissions = TeamPermissionsHelper.GetDefaultModeratorPermissions(),
                    Members = new List<string> { session.GenerateRavenId<Member>() },
                    Description = "Members in this team can manage issues.",
                    Color = "#F51A1A"
                };

                session.Store(adminsTeam);
                session.Store(moderatorsTeam);

                var adminMember = new Member
                {
                    Id = adminsTeam.Members.First(),
                    TeamId = adminsTeam.Id,
                    UserAuthId = command.User.AuthUserId
                };

                session.Store(adminMember);

                var moderatorMember = new Member
                {
                    Id = moderatorsTeam.Members.First(),
                    TeamId = moderatorsTeam.Id,
                    UserAuthId = command.User.AuthUserId
                };

                session.Store(moderatorMember);

                session.SaveChanges();

                Sender.Tell(new PayloadEvent(command) { Payload = true });
            });

            Receive<ChangePermissionCommand>(command =>
            {
                if (!TeamPermissionsHelper.AllPermissions.Contains(command.Permission))
                {
                    Sender.Tell(new PayloadEvent(command) { Errors = new []{ "Invalid permission" } });
                    return;
                }

                using var session = DocumentStoreSingleton.Store.OpenSession();

                var ravenTeamId = command.TeamId.ConvertToRavenId<Team>();
                var team = session.Load<Team>(ravenTeamId);

                if (team == null)
                {
                    Sender.Tell(new PayloadEvent(command) { Errors = new[] { "Team unavailable" } });
                    return;
                }

                if (command.Enabled)
                {
                    team.Permissions.Add(command.Permission);
                }
                else
                {
                    var permissions = team.Permissions.Where(p => !p.Equals(command.Permission)).ToHashSet();
                    team.Permissions = permissions;
                }

                session.Store(team);
                session.SaveChanges();

                Sender.Tell(new PayloadEvent(command) { Payload = true });
            });
        }
    }
}