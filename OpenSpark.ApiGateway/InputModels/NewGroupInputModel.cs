using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenSpark.ApiGateway.InputModels
{
    public class NewGroupInputModel
    {
        [MaxLength(40, ErrorMessage = "Group name must be less than or equal to 40 characters.")]
        [Required(ErrorMessage = "Please give your group a name.")]
        public string Name { get; set; }

        public string About { get; set; }

        [RegularExpression("Public|Private|Unlisted", ErrorMessage = "Invalid visibility value selected.")]
        public string Visibility { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public string CategoryId { get; set; }

        public List<string> Tags { get; set; }

        public List<string> Connected { get; set; }

        [Required]
        public string ConnectionId { get; set; }

        [Required]
        public string Callback { get; set; }
    }
}