using System.ComponentModel.DataAnnotations;

namespace Intellegens.Commons.DemoApi.Models
{
    public class Person
    {
        [Required]
        public string FullName { get; set; }

        [Key]
        public int Id { get; set; }
    }
}