using System.ComponentModel.DataAnnotations;

namespace OpenSpark.ApiGateway.InputModels
{
    public class ChangePermissionInputModel
    {
        [Required]
        public string TeamId { get; set; }

        public bool Enabled { get; set; }
        [Required]
        public string Permission { get; set; }
        [Required]
        public string ConnectionId { get; set; }
        [Required]
        public string Callback { get; set; }
    }
}