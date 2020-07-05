using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Reply
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int CommentId { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string When { get; set; }
        public int Votes { get; set; }
        public List<Reply> Replies { get; set; }
    }
}
