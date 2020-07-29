using System.Collections.Generic;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class NewsFeedQuery : IQuery
    {
        public User User { get; set; }
        public MetaData MetaData { get; set; }
        public List<string> Seen { get; set; }
    }
}