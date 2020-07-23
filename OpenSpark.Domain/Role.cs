using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Role : INamedEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public HashSet<string> PermissionIds { get; set; }
    }
}