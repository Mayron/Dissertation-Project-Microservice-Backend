using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenSpark.ApiGateway.InputModels
{
    public class NewProjectInputModel
    {
        [MaxLength(40, ErrorMessage = "Project name must be less than or equal to 40 characters.")]
        [Required(ErrorMessage = "Please give your project a name.")]
        public string Name { get; set; }

        public string About { get; set; }
        public List<string> Tags { get; set; }

        [RegularExpression("Public|Private|Unlisted", ErrorMessage = "Invalid visibility value selected.")]
        public string Visibility { get; set; }
    }
}
