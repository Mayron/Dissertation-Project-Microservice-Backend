﻿using System;
using System.Collections.Generic;

namespace OpenSpark.Shared.Domain
{
    public class User
    {
        // from Firebase auth
        public string AuthUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public List<string> Projects { get; set; } = new List<string>();
        public List<string> Groups { get; set; } = new List<string>();
        public bool EmailVerified { get; set; }
        public DateTime LastOnline { get; set; }
        public bool IsOnline { get; set; }
    }
}
