using System;

namespace OpenSpark.Domain
{
    public class Comment : IEntity
    {
        public string Id { get; set; }
        public string PostId { get; set; }
        public string Body { get; set; }
        public string AuthorUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Votes { get; set; }

        //        public List<Reply> Replies { get; set; }
    }
}