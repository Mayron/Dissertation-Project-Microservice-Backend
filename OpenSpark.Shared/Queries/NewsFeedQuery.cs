﻿using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class NewsFeedQuery : IQuery
    {
        public string ConnectionId { get; set; }
        public User User { get; set; }
    }
}