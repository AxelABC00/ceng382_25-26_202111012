/*using System.ComponentModel.DataAnnotations;

namespace MyRazorApp.Models
{
    public class ClassInformationModel
    {
        private int _id;

        public int Id
        {
            get => _id;
            set => _id = value; 
        }

        [Required(ErrorMessage = "Class Name is required.")]
        public string? ClassName { get; set; }

        [Required(ErrorMessage = "Student Count is required.")]
        [Range(1, 100, ErrorMessage = "Student Count must be between 1 and 100.")]
        public int StudentCount { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string? Description { get; set; }
    }
}
*/
using System.ComponentModel.DataAnnotations;

namespace MyRazorApp.Models
{
    public class ClassInformationModel
    {
        [Key]
        public int Id { get; set; } 

        [Required(ErrorMessage = "Class Name is required.")]
        [StringLength(50, ErrorMessage = "Class Name cannot exceed 50 characters.")]
        public string ?ClassName { get; set; }

        [Required(ErrorMessage = "Student Count is required.")]
        [Range(1, 100, ErrorMessage = "Student Count must be between 1 and 100.")]
        public int StudentCount { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string Description { get; set; } = "No description provided.";
    }
}
