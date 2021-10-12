using Intellegens.Commons.Api;
using Intellegens.Commons.DemoApi.Dto;
using Intellegens.Commons.DemoApi.Models;
using Intellegens.Commons.Services;
using Microsoft.AspNetCore.Mvc;

namespace Intellegens.Commons.DemoApi.Controllers
{
    [Route("/api/v1/courses")]
    public class CoursesControllerApi : CrudApiControllerAbstract<Course, CourseDto>
    {
        public CoursesControllerApi(IRepositoryBase<CourseDto> repositoryBase) : base(repositoryBase)
        {
        }
    }
}