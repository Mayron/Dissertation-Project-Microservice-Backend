using System.ComponentModel.DataAnnotations;

namespace OpenSpark.ApiGateway.InputModels
{
    public class NewPostInputModel
    {
        [Required]
        public string Title { get; set; }

        public string Body { get; set; }

        [Required]
        public string GroupId { get; set; }

        [Required]
        public string ConnectionId { get; set; }

        [Required]
        public string Callback { get; set; }
    }
}
