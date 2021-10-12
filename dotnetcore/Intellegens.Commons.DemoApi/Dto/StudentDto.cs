using System;

namespace Intellegens.Commons.DemoApi.Dto
{
    public class StudentDto : BaseDto
    {
        public string FullName { get; set; }

        public int PersonId { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}