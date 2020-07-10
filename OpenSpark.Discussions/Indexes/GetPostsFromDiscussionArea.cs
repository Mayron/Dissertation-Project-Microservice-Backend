using System.Collections.Generic;
using System.Linq;
using OpenSpark.Domain;
using Raven.Client.Documents.Indexes;

namespace OpenSpark.Discussions.Indexes
{
    public class GetPostsFromDiscussionArea : AbstractIndexCreationTask<DiscussionArea, GetPostsFromDiscussionArea.Result>
    {
        public class Result
        {
            public bool IsPublic { get; set; }
            public string AreaId { get; set; }
            public Post Post { get; set; }
        }

        public GetPostsFromDiscussionArea()
        {
            Map = discussions => 
                from discussion in discussions
                from post in discussion.Posts
                select new
                {
                    discussion.IsPublic,
                    discussion.AreaId,
                    Post = post
                };

            Reduce = results => 
                from result in results
                group result by result.AreaId into g
                select new 
                {
                    IsPublic = g.Select(g1 => g1.IsPublic).FirstOrDefault(),
                    AreaId = g.Key,
                    Post = g.Select(g1 => g1.Post).FirstOrDefault(),
                };
        }
    }
}
