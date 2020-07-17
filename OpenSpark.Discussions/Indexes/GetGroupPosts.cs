using OpenSpark.Domain;
using Raven.Client.Documents.Indexes;
using System;
using System.Linq;

namespace OpenSpark.Discussions.Indexes
{
    public class GetGroupPosts : AbstractIndexCreationTask<GroupPosts, GetGroupPosts.Result>
    {
        public class Result
        {
            public bool IsPublic { get; set; }
            public string GroupId { get; set; }
            public string PostId { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
            public string AuthorUserId { get; set; }
            public DateTime CreatedAt { get; set; }
            public int TotalComments { get; set; }
            public int Votes { get; set; }
        }

        public GetGroupPosts()
        {
            Map = groups =>
                from g in groups
                from post in g.Posts
                select new
                {
                    g.IsPublic,
                    g.GroupId,
                    PostId = post.Id,
                    post.Title,
                    post.Body,
                    post.AuthorUserId,
                    post.CreatedAt,
                    TotalComments = post.Comments.Count,
                    post.Votes
                };
        }
    }
}