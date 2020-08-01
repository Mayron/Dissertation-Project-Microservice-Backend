using System.ComponentModel.DataAnnotations;

namespace OpenSpark.ApiGateway.InputModels
{
    public class ConnectProjectInputModel
    {
        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Invalid project id")]
        public string ProjectId { get; set; }

        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Invalid group id")]
        public string GroupId { get; set; }

        [Required]
        public string ConnectionId { get; set; }

        [Required]
        public string Callback { get; set; }
    }
}