using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Models
{
    public interface ISagaStateData
    {
        Guid TransactionId { get; set; }
    }
}
