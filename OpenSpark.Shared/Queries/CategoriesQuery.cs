using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Queries
{
    public class CategoriesQuery : IQuery
    {
        public Guid Id { get; set; }
        public string Callback { get; set; }
        public string ConnectionId { get; set; }
        public User User { get; set; }
    }
}
