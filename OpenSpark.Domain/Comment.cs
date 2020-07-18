using System;
using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string When { get; set; }
        public int Votes { get; set; }
        public List<Reply> Replies { get; set; }
    }
}
