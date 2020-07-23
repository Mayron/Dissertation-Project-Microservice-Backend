﻿using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Posts
{
    public class CreatePostCommand : ICommand
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string GroupId { get; set; }
        public User User { get; set; }
    }
}
