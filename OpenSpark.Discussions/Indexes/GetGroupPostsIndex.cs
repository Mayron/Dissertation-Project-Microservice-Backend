using OpenSpark.Domain;
using Raven.Client.Documents.Indexes;
using System;
using System.Linq;
using Akka.Actor;

namespace OpenSpark.Discussions.Indexes
{
    public class GetGroupPostsIndex : AbstractIndexCreationTask<GroupPosts, GetGroupPostsIndex.Result>
    {
        public class Result
        {
            public bool IsPrivate { get; set; }
            public string GroupId { get; set; }
            public string PostId { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
            public string AuthorUserId { get; set; }
            public DateTime CreatedAt { get; set; }
            public int TotalComments { get; set; }
            public int Votes { get; set; }
        }

        public GetGroupPostsIndex()
        {
            Map = groups =>
                from g in groups
                from post in g.Posts
                select new Result
                {
                    IsPrivate = g.IsPrivate,
                    GroupId = g.GroupId,

                    PostId = post.Id,
                    Title = post.Title,
                    Body = post.Body,
                    AuthorUserId = post.AuthorUserId,
                    CreatedAt = post.CreatedAt,
                    TotalComments = post.Comments.Count,
                    Votes = post.Votes
                };

            Reduce = results => from result in results
                group result by new { result.PostId } into g
                select new
                {
                    g.Key.PostId,
                    g.FirstOrDefault().IsPrivate,
                    g.FirstOrDefault().GroupId,
                    g.FirstOrDefault().Title,
                    g.FirstOrDefault().Body,
                    g.FirstOrDefault().AuthorUserId,
                    g.FirstOrDefault().CreatedAt,
                    g.FirstOrDefault().TotalComments,
                    g.FirstOrDefault().Votes
                };
        }
    }
}