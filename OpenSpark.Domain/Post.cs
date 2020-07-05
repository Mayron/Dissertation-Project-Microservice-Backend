using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Post
    {
        public string Url { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string When { get; set; }
        public int Votes { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
