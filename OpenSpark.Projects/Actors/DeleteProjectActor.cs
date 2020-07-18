using Akka.Actor;
using OpenSpark.Shared.Commands.Projects;
using OpenSpark.Shared.Events.Sagas.CreatePost;
using System;

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
                    TransactionId = command.TransactionId,
                    ProjectId = command.ProjectId
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }
    }
}