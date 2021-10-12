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
                .ForMember(x => x.FullName, o => { o.MapFrom(s => s.Person.FullName); })
                .ForMember(x => x.DateOfBirth, o => { o.MapFrom(s => s.Person.DateOfBirth); });

            CreateMap<StudentDto, Person>()
                .ForMember(x => x.DateOfBirth, o => o.MapFrom(x => x.DateOfBirth))
                .ForMember(x => x.Id, o => o.MapFrom(x => x.PersonId))
                .ForMember(x => x.FullName, o => o.MapFrom(x => x.FullName));

            CreateMap<StudentDto, Student>()
                .ForMember(x => x.Person, o => o.MapFrom(x => x));

            CreateMap<StudentCourse, StudentCourseDto>();
            CreateMap<StudentCourseDto, StudentCourse>()
                .ForMember(x => x.Course, o => o.Ignore())
                .ForMember(x => x.Student, o => o.Ignore());
        }
    }
}