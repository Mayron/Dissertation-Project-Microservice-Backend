using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.Groups;
using System;
using OpenSpark.Shared.Events.CreateGroup;

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
                    GroupId = command.GroupId
                });

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }
    }
}