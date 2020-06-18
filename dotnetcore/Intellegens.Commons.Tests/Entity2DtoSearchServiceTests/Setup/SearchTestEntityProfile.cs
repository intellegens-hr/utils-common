using AutoMapper;
using Intellegens.Commons.Tests.SearchTests.Setup;

namespace Intellegens.Commons.Tests.Entity2DtoSearchServiceTests.Setup
{
    public class SearchTestEntityProfile : Profile
    {
        public SearchTestEntityProfile()
        {
            CreateMap<SearchTestEntity, SearchTestEntityDto>()
                .ForMember(x => x.ChildrenDtos, opt =>
                {
                    opt.MapFrom(m => m.Children);
                    opt.AllowNull();
                })
                .ForMember(x => x.SiblingDto, opt =>
                {
                    opt.MapFrom(m => m.Sibling);
                    opt.AllowNull();
                })
                .ForAllMembers(x => x.AllowNull());

            CreateMap<SearchTestEntityDto, SearchTestEntity>()
                .ForMember(x => x.Children, opt =>
                {
                    opt.MapFrom(m => m.ChildrenDtos);
                    opt.AllowNull();
                })
                .ForMember(x => x.Sibling, opt =>
                {
                    opt.MapFrom(m => m.SiblingDto);
                    opt.AllowNull();
                });

            CreateMap<SearchTestChildEntity, SearchTestChildEntityDto>()
                .ForMember(x => x.ParentDto, opt => opt.MapFrom(m => m.Parent))
                .ForAllMembers(x => x.AllowNull());

            CreateMap<SearchTestChildEntityDto, SearchTestChildEntity>()
                .ForMember(x => x.Parent, opt => opt.MapFrom(m => m.ParentDto));
        }
    }
}