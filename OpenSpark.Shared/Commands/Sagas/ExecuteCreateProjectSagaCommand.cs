﻿using System.Collections.Generic;
using OpenSpark.Shared.Domain;

namespace OpenSpark.Shared.Commands.Sagas
{
    public class ExecuteCreateProjectSagaCommand : ISagaExecutionCommand
    {
        public User User { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public List<string> Tags { get; set; }
        public string Visibility { get; set; }
        public MetaData MetaData { get; set; }
    }
}