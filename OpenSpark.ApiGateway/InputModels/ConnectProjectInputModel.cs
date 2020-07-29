using System.ComponentModel.DataAnnotations;

namespace OpenSpark.ApiGateway.InputModels
{
    public class ConnectProjectInputModel
    {
        [Required]
        public string ProjectId { get; set; }

        [Required]
        public string GroupId { get; set; }

        [Required]
        public string ConnectionId { get; set; }

        [Required]
        public string Callback { get; set; }
    }
}