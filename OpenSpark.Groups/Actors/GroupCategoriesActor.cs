using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;

namespace OpenSpark.Groups.Actors
{
    public class GroupCategoriesActor : ReceiveActor
    {
        private IImmutableList<GroupCategory> _groupCategories;

        public GroupCategoriesActor()
        {
            _groupCategories = ImmutableList<GroupCategory>.Empty;

            Receive<GroupCategoriesQuery>(query =>
            {
                if (_groupCategories.Count == 0)
                {
                    using var session = DocumentStoreSingleton.Store.OpenSession();
                    _groupCategories = session.Query<GroupCategory>().ToImmutableList();
                }

                Sender.Tell(new PayloadEvent
                {
                    ConnectionId = query.ConnectionId,
                    Callback = query.Callback,
                    Payload = _groupCategories
                        .Select(c => new KeyValuePair<string, string>(c.Name, c.Id))
                        .ToList()
                });
            });
        }
    }
}