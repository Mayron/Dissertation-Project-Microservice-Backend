using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Routing;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Teams;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.RavenDb;
using OpenSpark.Shared.ViewModels;
using OpenSpark.Teams.Domain;

namespace OpenSpark.Teams.Actors
{
    internal class CreateTeamActor : ReceiveActor
    {
        public static Props Props { get; } = Props.Create<CreateTeamActor>()
            .WithRouter(new RoundRobinPool(2,
                new DefaultResizer(1, 5)));

        public CreateTeamActor()
        {
            Receive<CreateTeamCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                var team = new Team
                {
                    Id = session.GenerateRavenIdFromName<Team>(command.TeamName),
                    Name = command.TeamName,
                    ProjectId = command.ProjectId,
                    PermissionIds = TeamPermissionsHelper.GetDefaultNewTeamPermissions(),
                    Members = new List<Member>
                    {
                        new Member
                        {
                            Id = command.User.AuthUserId.ConvertToRavenId<Member>()
                        }
                    },
                    Description = command.Description,
                    Color = command.Color
                };

                session.Store(team);
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
                    Id = session.GenerateRavenIdFromName<Team>("Admins"),
                    Name = "Admins",
                    ProjectId = command.ProjectId,
                    PermissionIds = TeamPermissionsHelper.GetDefaultAdminPermissions(),
                    Members = new List<Member>
                    {
                        new Member
                        {
                            Id = command.User.AuthUserId.ConvertToRavenId<Member>()
                        }
                    },
                    Description = "Members in this team have full access to all aspects of the project, including editing team permissions.",
                    Color = "#FF4F00"
                };

                var moderatorsTeam = new Team
                {
                    Id = session.GenerateRavenIdFromName<Team>("Moderators"),
                    Name = "Moderators",
                    ProjectId = command.ProjectId,
                    PermissionIds = TeamPermissionsHelper.GetDefaultModeratorPermissions(),
                    Members = new List<Member>
                    {
                        new Member
                        {
                            Id = command.User.AuthUserId.ConvertToRavenId<Member>()
                        }
                    },
                    Description = "Members in this team can manage issues.",
                    Color = "#F51A1A"
                };

                session.Store(adminsTeam);
                session.Store(moderatorsTeam);
                session.SaveChanges();

                Sender.Tell(new PayloadEvent(command) { Payload = true });
            });
        }
    }
}