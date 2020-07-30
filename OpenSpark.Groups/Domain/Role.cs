using System.Collections.Generic;
using OpenSpark.Shared.Domain;

namespace OpenSpark.Groups.Domain
{
    public class Role : INamedEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public HashSet<string> PermissionIds { get; set; }
    }
}