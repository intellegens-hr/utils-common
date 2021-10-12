using System.Collections.Generic;

namespace Intellegens.Commons.DemoApi.Dto
{
    public class LecturerDto : BaseDto
    {
        public IEnumerable<CourseDto> Courses { get; set; }

        public int PersonId { get; set; }
    }
}