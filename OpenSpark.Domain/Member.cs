using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSpark.Domain
{
    public class Member
    {
        public string UserId { get; set; }

        // Should not contain implicit roles!
        public List<Guid> RoleIds { get; set; }
        public string GroupId { get; set; }
    }
}
