using Akka.Actor;
using OpenSpark.Shared.Commands.Projects;
using System;
using OpenSpark.Shared.Events.CreatePost;

namespace OpenSpark.Projects.Actors
{
    public class DeleteProjectActor : ReceiveActor
    {
        public DeleteProjectActor()
        {
            Receive<DeleteProjectCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                session.Delete(command.ProjectId);
                session.SaveChanges();

                Sender.Tell(new ProjectDeletedEvent
                {
                    ProjectId = command.ProjectId
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }
    }
}