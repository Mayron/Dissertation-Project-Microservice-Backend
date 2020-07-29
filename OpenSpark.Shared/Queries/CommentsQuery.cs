using System.Collections.Generic;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class CommentsQuery : IQuery
    {
        public string PostId { get; set; }
        public List<string> Seen { get; set; }
        public User User { get; set; }
        public MetaData MetaData { get; set; }
    }
}