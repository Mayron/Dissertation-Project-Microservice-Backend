using System;
using OpenSpark.Shared;

namespace OpenSpark.ApiGateway.Models
{
    public class Uninitialized : ISagaStateData
    {
        public static Uninitialized Instance = new Uninitialized();
        public Guid TransactionId { get; set; } = Guid.Empty;
    }
}