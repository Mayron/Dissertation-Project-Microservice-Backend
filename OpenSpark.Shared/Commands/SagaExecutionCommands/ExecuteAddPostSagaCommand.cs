﻿using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.SagaExecutionCommands
{
    public class ExecuteAddPostSagaCommand : ISagaExecutionCommand
    {
        public Guid TransactionId { get; set; }
        public Post Post { get; set; }
        public string GroupId { get; set; }
        public User User { get; set; }
        public string SagaName { get; set; }
    }
}