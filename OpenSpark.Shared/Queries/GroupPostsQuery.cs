using System.Collections.Generic;
using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Queries
{
    public class GroupPostsQuery : IQuery
    {
        public User User { get; set; }
        public MetaData MetaData { get; set; }
        public string GroupId { get; set; }
        public List<string> Seen { get; set; }
        public string PostId { get; set; }
    }
}