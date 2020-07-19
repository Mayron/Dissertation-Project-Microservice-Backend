using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Events.Sagas;
using OpenSpark.Shared.Events.Sagas.CreateProject;
using OpenSpark.Shared.RavenDb;
using System;
using System.Collections.Generic;

namespace OpenSpark.Projects.Actors
{
    public class CreateProjectActor : ReceiveActor
    {
        public CreateProjectActor()
        {
            Receive<CreateProjectCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                if (session.IsNameTaken<Project>(command.Name))
                {
                    Sender.Tell(new SagaErrorEvent
                    {
                        Message = "Project name taken",
                        TransactionId = command.TransactionId
                    });

                    Self.GracefulStop(TimeSpan.FromSeconds(5));
                    return;
                }

                var newProjectId = session.GenerateRavenId<Project>();
                var project = new Project
                {
                    Id = newProjectId,
                    OwnerUserId = command.User.AuthUserId,
                    About = command.About,
                    Name = command.Name,
                    Tags = command.Tags,
                    Visibility = VisibilityStatus.Public, // TODO: Needs to be configurable on creation
                    CreatedAt = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    Subscribers = new List<string>(),
                    TeamMembers = new List<string>()
                };

                session.Store(project);
                session.SaveChanges();

                Sender.Tell(new ProjectCreatedEvent
                {
                    TransactionId = command.TransactionId,
                    Project = project
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }
    }
}