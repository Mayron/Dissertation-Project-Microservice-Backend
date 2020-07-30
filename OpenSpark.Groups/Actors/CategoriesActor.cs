using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using OpenSpark.Groups.Domain;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;

namespace OpenSpark.Groups.Actors
{
    public class CategoriesActor : ReceiveActor
    {
        public CategoriesActor()
        {
            var categories = ImmutableList<Category>.Empty;

            Receive<CategoriesQuery>(query =>
            {
                if (categories.Count == 0)
                {
                    using var session = DocumentStoreSingleton.Store.OpenSession();
                    categories = session.Query<Category>().ToImmutableList();
                }

                var dropdownOptions = categories.Select(c =>
                    new KeyValuePair<string, string>(c.Name, c.Id)).ToList();

                Sender.Tell(new PayloadEvent(query)
                {
                    Payload = dropdownOptions
                });
            });
        }
    }
}
