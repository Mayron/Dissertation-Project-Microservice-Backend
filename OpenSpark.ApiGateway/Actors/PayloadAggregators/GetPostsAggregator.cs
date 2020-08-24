using Akka.Actor;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.Shared.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace OpenSpark.ApiGateway.Actors.PayloadAggregators
{
    public class GetPostsAggregator : BaseMultiQueryAggregatorActor
    {
        public GetPostsAggregator(MultiQueryContext context, IActorRef callback) : base(context, callback)
        {
        }

        protected override object GetAggregatedPayload(List<object> payloads)
        {
            var posts = payloads
                .Where(r => r is List<PostViewModel>)
                .Cast<List<PostViewModel>>()
                .Single();

            var groups = payloads
                .Where(s => s is NamedEntityViewModel)
                .Cast<NamedEntityViewModel>()
                .ToList();

            foreach (var post in posts)
            {
                var group = groups.First(g => post.GroupId == g.Id);
                post.GroupName = group.Name;
            }

            return posts;
        }
    }
}