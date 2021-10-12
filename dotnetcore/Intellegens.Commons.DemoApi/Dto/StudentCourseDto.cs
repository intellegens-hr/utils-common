namespace Intellegens.Commons.DemoApi.Dto
{
    public class StudentCourseDto : BaseDto
    {
        public CourseDto Course { get; set; }
        public int CourseId { get; set; }

        public int StudentId { get; set; }
    }
}