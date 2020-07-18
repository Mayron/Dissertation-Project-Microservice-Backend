using OpenSpark.Domain;
using System;

namespace OpenSpark.ApiGateway.Models.StateData.CreateProject
{
    public class ProcessingStateData : ISagaStateData
    {
        public Guid TransactionId { get; set; }
        public User User { get; set; }
        public string ProjectId { get; set; }
    }
}