using System.ComponentModel.DataAnnotations;

namespace OpenSpark.ApiGateway.InputModels
{
    public class NewTeamInputModel
    {
        [MaxLength(40, ErrorMessage = "Team name must be less than or equal to 40 characters.")]
        [Required(ErrorMessage = "Please give your team a name.")]
        public string Name { get; set; }

        [MaxLength(250, ErrorMessage = "Team description must be less than or equal to 250 characters.")]
        public string Description { get; set; }

        [MaxLength(7, ErrorMessage = "Invalid color code")]
        public string Color { get; set; }

        [Required]
        public string ProjectId { get; set; }

        [Required]
        public string ConnectionId { get; set; }

        [Required]
        public string Callback { get; set; }
    }
}