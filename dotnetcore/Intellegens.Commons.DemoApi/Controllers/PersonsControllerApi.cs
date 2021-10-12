using Intellegens.Commons.Api;
using Intellegens.Commons.DemoApi.Dto;
using Intellegens.Commons.DemoApi.Models;
using Intellegens.Commons.Services;
using Microsoft.AspNetCore.Mvc;

namespace Intellegens.Commons.DemoApi.Controllers
{
    [Route("/api/v1/persons")]
    public class PersonsControllerApi : CrudApiControllerAbstract<Person, PersonDto>
    {
        public PersonsControllerApi(IRepositoryBase<PersonDto> repositoryBase) : base(repositoryBase)
        {
        }
    }
}