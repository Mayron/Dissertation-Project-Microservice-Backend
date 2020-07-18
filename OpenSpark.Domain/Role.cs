using System;
using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class Role
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; }
        public List<Guid> PermissionIds { get; set; }
    }
}