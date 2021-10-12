using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intellegens.Commons.DemoApi.Models
{
    public class StudentCourse
    {
        public virtual Course Course { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseId { get; set; }

        [Key]
        public int Id { get; set; }

        public virtual Student Student { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }
    }
}