using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Role : INamedEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> PermissionIds { get; set; }
    }
}