using Akka.Actor;
using Akka.Routing;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Teams;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.RavenDb;
using OpenSpark.Teams.Domain;
using System.Collections.Generic;

namespace OpenSpark.Teams.Actors
{
    internal class TeamCommandPool : ReceiveActor
    {
        public static Props Props { get; } = Props.Create<TeamCommandPool>()
            .WithRouter(new RoundRobinPool(2,
                new DefaultResizer(1, 5)));

        public TeamCommandPool()
        {
            Receive<CreateTeamCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();
                var ravenMemberId = session.GenerateRavenId<Member>();

                var team = new Team
                {
                    Id = session.GenerateRavenIdFromName<Team>(command.TeamName),
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
                var ravenMemberId = session.GenerateRavenId<Member>();

                var adminsTeam = new Team
                {
                    Id = session.GenerateRavenIdFromName<Team>("Admins"),
                    Name = "Admins",
                    ProjectId = command.ProjectId,
                    Permissions = TeamPermissionsHelper.GetDefaultAdminPermissions(),
                    Members = new List<string> { ravenMemberId },
                    Description = "Members in this team have full access to all aspects of the project, including editing team permissions.",
                    Color = "#FF4F00"
                };

                var moderatorsTeam = new Team
                {
                    Id = session.GenerateRavenIdFromName<Team>("Moderators"),
                    Name = "Moderators",
                    ProjectId = command.ProjectId,
                    Permissions = TeamPermissionsHelper.GetDefaultModeratorPermissions(),
                    Members = new List<string> { ravenMemberId },
                    Description = "Members in this team can manage issues.",
                    Color = "#F51A1A"
                };

                session.Store(adminsTeam);
                session.Store(moderatorsTeam);

                var adminMember = new Member
                {
                    Id = ravenMemberId,
                    TeamId = adminsTeam.Id,
                    UserAuthId = command.User.AuthUserId
                };

                session.Store(adminMember);

                var moderatorMember = new Member
                {
                    Id = ravenMemberId,
                    TeamId = moderatorsTeam.Id,
                    UserAuthId = command.User.AuthUserId
                };

                session.Store(moderatorMember);

                session.SaveChanges();

                Sender.Tell(new PayloadEvent(command) { Payload = true });
            });
        }
    }
}