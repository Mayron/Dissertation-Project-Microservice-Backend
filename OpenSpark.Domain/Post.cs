﻿using System;
using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Post
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string AuthorUserId { get; set; }
        public string When { get; set; }
        public int Votes { get; set; }
        public List<Comment> Comments { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
