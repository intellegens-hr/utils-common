using Intellegens.Commons.Api;
using Intellegens.Commons.DemoApi.Dto;
using Intellegens.Commons.DemoApi.Models;
using Intellegens.Commons.Services;
using Microsoft.AspNetCore.Mvc;

namespace Intellegens.Commons.DemoApi.Controllers
{
    [Route("/api/v1/lecturers")]
    public class LecturerControllerApi : CrudApiControllerAbstract<Lecturer, LecturerDto>
    {
        public LecturerControllerApi(IRepositoryBase<LecturerDto> repositoryBase) : base(repositoryBase)
        {
        }
    }
}