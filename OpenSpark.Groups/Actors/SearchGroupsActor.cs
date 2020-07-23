using Akka.Actor;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using System.Linq;
using Akka.Routing;
using Group = OpenSpark.Domain.Group;

namespace OpenSpark.Groups.Actors
{
    public class SearchGroupsActor : ReceiveActor
    {
        public SearchGroupsActor()
        {
            Receive<SearchGroupsQuery>(query =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                var searchTerms = query.SearchQuery
                    .Split(" ")
                    .Select(t => $"*{t}*")
                    .ToArray();

                var searchQuery = session.Query<Group>()
                    .Search(g => g.Name, searchTerms, boost: 10)
                    .Search(g => g.About, searchTerms);

                if (query.User != null)
                {
                    // include private groups
                    var userGroups = query.User.Groups;
                    searchQuery = searchQuery.Where(g =>
                        g.Visibility == VisibilityStatus.Public || g.Id.In(userGroups));
                }
                else
                {
                    searchQuery = searchQuery.Where(g => g.Visibility == VisibilityStatus.Public);
                }

                var groups = searchQuery
                    .Select(g => new Group
                    {
                        Id = g.Id,
                        Name = g.Name
                    }).ToList();

                foreach (var g in groups) g.Id = g.Id.ConvertToEntityId();

                Sender.Tell(new PayloadEvent(query)
                {
                    Payload = groups
                });
            });
        }

        public static Props Props { get; } = Props.Create<SearchGroupsActor>()
            .WithRouter(new RoundRobinPool(5,
                new DefaultResizer(1, 10)));
    }
}