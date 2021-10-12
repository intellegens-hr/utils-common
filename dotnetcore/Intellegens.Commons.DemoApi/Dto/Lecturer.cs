using System;
using System.Collections.Generic;

namespace Intellegens.Commons.DemoApi.Dto
{
    public class LecturerDto : BaseDto
    {
        public IEnumerable<CourseDto> Courses { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string FullName { get; set; }
        public int PersonId { get; set; }
    }
}