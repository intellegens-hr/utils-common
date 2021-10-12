﻿using AutoMapper;
using Intellegens.Commons.DemoApi.DbContext;
using Intellegens.Commons.DemoApi.Dto;
using Intellegens.Commons.DemoApi.Models;
using Intellegens.Commons.Search;
using Intellegens.Commons.Services;

namespace Intellegens.Commons.DemoApi.Services
{
    public class StudentCourseService : RepositoryBase<StudentCourse, StudentCourseDto>
    {
        public StudentCourseService(DemoDbContext dbContext, IMapper mapper, ISearchServiceFactory searchServiceFactory) : base(dbContext, mapper, searchServiceFactory)
        {
        }
    }
}