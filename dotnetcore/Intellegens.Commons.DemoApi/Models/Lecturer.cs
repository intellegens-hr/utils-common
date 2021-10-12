using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Intellegens.Commons.DemoApi.Models
{
    public class Lecturer
    {
        [InverseProperty(nameof(Lecturer))]
        public virtual ICollection<Course> Courses { get; set; }

        [Key]
        public int Id { get; set; }

        public virtual Person Person { get; set; }

        [ForeignKey(nameof(Person))]
        public int PersonId { get; set; }
    }
}