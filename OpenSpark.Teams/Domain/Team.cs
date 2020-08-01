using System.Collections.Generic;
using OpenSpark.Shared.Domain;

namespace OpenSpark.Teams.Domain
{
    public class Team : INamedEntity
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Members { get; set; }
        public string Color { get; set; }
        public HashSet<string> Permissions { get; set; }
    }
}