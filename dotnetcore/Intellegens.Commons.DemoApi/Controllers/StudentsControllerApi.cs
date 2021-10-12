using Intellegens.Commons.Api;
using Intellegens.Commons.DemoApi.Dto;
using Intellegens.Commons.DemoApi.Models;
using Intellegens.Commons.Services;
using Microsoft.AspNetCore.Mvc;

namespace Intellegens.Commons.DemoApi.Controllers
{
    [Route("/api/v1/students")]
    public class StudentsControllerApi : CrudApiControllerAbstract<Student, StudentDto>
    {
        public StudentsControllerApi(IRepositoryBase<StudentDto> repositoryBase) : base(repositoryBase)
        {
        }
    }
}