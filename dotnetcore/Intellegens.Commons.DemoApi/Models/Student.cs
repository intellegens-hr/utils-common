using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intellegens.Commons.DemoApi.Models
{
    public class Student
    {
        [InverseProperty(nameof(Student))]
        public virtual ICollection<StudentCourse> Courses { get; set; }

        [Key]
        public int Id { get; set; }

        public virtual Person Person { get; set; }

        [ForeignKey(nameof(Person))]
        public int PersonId { get; set; }
    }
}