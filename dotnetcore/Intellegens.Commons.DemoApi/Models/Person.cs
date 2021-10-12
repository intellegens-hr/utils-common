using System;
using System.ComponentModel.DataAnnotations;

namespace Intellegens.Commons.DemoApi.Models
{
    public class Person
    {
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string FullName { get; set; }

        [Key]
        public int Id { get; set; }
    }
}