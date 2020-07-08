using System.Collections.Generic;

namespace OpenSpark.Domain
{
    public class User
    {
        // from Firebase auth
        public string UserId { get; set; }
        public string CreatedAt { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public List<string> Projects { get; set; }
        public List<string> Groups { get; set; }
    }
}
