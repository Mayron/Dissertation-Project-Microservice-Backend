using System.Collections.Generic;

namespace OpenSpark.ApiGateway.InputModels
{
    public class NewGroupInputModel
    {
        public string Name { get; set; }
        public string About { get; set; }
        public string CategoryId { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Connected { get; set; }
        public string OwnerUserId { get; set; }
    }
}
