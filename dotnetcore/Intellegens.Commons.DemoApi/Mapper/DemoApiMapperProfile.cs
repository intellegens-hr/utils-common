using AutoMapper;
using Intellegens.Commons.DemoApi.Dto;
using Intellegens.Commons.DemoApi.Models;

namespace Intellegens.Commons.DemoApi.Mapper
{
    public class DemoApiMapperProfile : Profile
    {
        public DemoApiMapperProfile()
        {
            CreateMap<Person, PersonDto>()
                .ReverseMap();

            CreateMap<Course, CourseDto>()
                .ReverseMap();

            CreateMap<Lecturer, LecturerDto>()
                .ForMember(x => x.Courses, o => { o.MapFrom(s => s.Courses); })
                .ReverseMap();

            CreateMap<Student, StudentDto>()
                .ForMember(x => x.FullName, o => { o.MapFrom(s => s.Person.FullName); });

            CreateMap<StudentDto, Student>()
                .ForMember(x => x.Person, o =>
                {
                    o.MapFrom(s => new Person
                    {
                        Id = s.PersonId,
                        FullName = s.FullName
                    });
                });
        }
    }
}