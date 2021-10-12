using System;

namespace Intellegens.Commons.DemoApi.Dto
{
    public class PersonDto : BaseDto
    {
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}