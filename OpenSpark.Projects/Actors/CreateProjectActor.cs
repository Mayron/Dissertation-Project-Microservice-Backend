using Akka.Actor;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.RavenDb;
using System;
using System.Collections.Generic;
using Akka.Routing;
using OpenSpark.Projects.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Events;
using OpenSpark.Shared.Events.CreateProject;

namespace OpenSpark.Projects.Actors
{
    public class CreateProjectActor : ReceiveActor
    {
        public static Props Props { get; } = Props.Create<CreateProjectActor>()
            .WithRouter(new RoundRobinPool(1,
                new DefaultResizer(1, 5)));

        public CreateProjectActor()
        {
            Receive<CreateProjectCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                if (session.IsNameTaken<Project>(command.Name))
                {
                    Sender.Tell(new ErrorEvent
                    {
                        Message = "Project name taken",
                    });

                    return;
                }

                var newProjectId = session.GenerateRavenIdFromName<Project>(command.Name);
                var project = new Project
                {
                    Id = newProjectId,
                    OwnerUserId = command.User.AuthUserId,
                    About = command.About,
                    Name = command.Name,
                    Tags = command.Tags,
                    Visibility = VisibilityHelper.GetCleanVisibility(command.Visibility),
                    CreatedAt = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    Subscribers = new List<string>(),
                    TeamMembers = new List<string>(),
                    LinkedGroups = new List<string>()
                };

                session.Store(project);
                session.SaveChanges();

                Sender.Tell(new ProjectCreatedEvent
                {
                    ProjectId = project.Id.ConvertToClientId()
                });
            });
        }
    }
}