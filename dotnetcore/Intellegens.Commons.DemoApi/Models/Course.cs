using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intellegens.Commons.DemoApi.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        public virtual Lecturer Lecturer { get; set; }

        [ForeignKey(nameof(Lecturer))]
        public int LecturerId { get; set; }

        [Required]
        public string Name { get; set; }

        [InverseProperty(nameof(Course))]
        public virtual ICollection<StudentCourse> StudentCourses { get; set; }
    }
}