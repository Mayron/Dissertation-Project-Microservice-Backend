﻿using System.Collections.Generic;

namespace OpenSpark.Discussions.Domain
{
    public class GroupPosts
    {
        public string GroupId { get; set; }
        public bool IsPrivate { get; set; }
        public List<Post> Posts { get; set; }
    }
}