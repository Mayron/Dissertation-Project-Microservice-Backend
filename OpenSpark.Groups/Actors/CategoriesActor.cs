using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;

namespace OpenSpark.Groups.Actors
{
    public class CategoriesActor : ReceiveActor
    {
        private IImmutableList<Category> _categories;

        public CategoriesActor()
        {
            _categories = ImmutableList<Category>.Empty;

            Receive<CategoriesQuery>(query =>
            {
                if (_categories.Count == 0)
                {
                    using var session = DocumentStoreSingleton.Store.OpenSession();
                    _categories = session.Query<Category>().ToImmutableList();
                }

                Sender.Tell(new PayloadEvent(query)
                {
                    Payload = _categories
                        .Select(c => new KeyValuePair<string, string>(c.Name, c.Id))
                        .ToList()
                });
            });
        }
    }
}