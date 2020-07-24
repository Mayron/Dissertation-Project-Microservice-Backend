﻿using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class CategoriesQuery : IQuery
    {
        public User User { get; set; }
        public QueryMetaData MetaData { get; set; }
    }
}