using System.Collections.Generic;

namespace OpenSpark.ApiGateway.InputModels
{
    public class NewProjectInputModel
    {
        public string Name { get; set; }
        public string About { get; set; }
        public List<string> Tags { get; set; }
    }
}
