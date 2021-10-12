using Intellegens.Commons.Api;
using Intellegens.Commons.DemoApi.Dto;
using Intellegens.Commons.DemoApi.Models;
using Intellegens.Commons.Services;
using Microsoft.AspNetCore.Mvc;

namespace Intellegens.Commons.DemoApi.Controllers
{
    [Route("/api/v1/student-courses")]
    public class StudentCoursesControllerApi : CrudApiControllerAbstract<StudentCourse, StudentCourseDto>
    {
        public StudentCoursesControllerApi(IRepositoryBase<StudentCourseDto> repositoryBase) : base(repositoryBase)
        {
        }
    }
}