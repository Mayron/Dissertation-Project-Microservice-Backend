﻿using System;
using System.Collections.Generic;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Groups
{
    public class CreateGroupCommand : ICommand
    {
        public Guid TransactionId { get; set; }
        public string Name { get; set; }
        public List<string> Connected { get; set; }
        public List<string> Tags { get; set; }
        public string CategoryId { get; set; }
        public string About { get; set; }
        public User User { get; set; }
    }
}