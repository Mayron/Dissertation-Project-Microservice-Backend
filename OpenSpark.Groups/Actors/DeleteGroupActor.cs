using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.Groups;
using OpenSpark.Shared.Events.Sagas.CreateGroup;
using System;

namespace OpenSpark.Groups.Actors
{
    public class DeleteGroupActor : ReceiveActor
    {
        public DeleteGroupActor()
        {
            Receive<DeleteGroupCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                var group = session.Include<Group>(g => g.Members)
                    .Load<Group>(command.GroupId);

                foreach (var memberId in group.Members)
                {
                    var member = session.Load<Member>(memberId);
                    session.Delete(member);
                }

                session.Delete(group);
                session.SaveChanges();

                Sender.Tell(new GroupDeletedEvent
                {
                    TransactionId = command.TransactionId,
                    GroupId = command.GroupId
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }
    }
}