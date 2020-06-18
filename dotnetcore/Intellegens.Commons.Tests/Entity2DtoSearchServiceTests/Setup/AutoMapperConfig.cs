using AutoMapper;

namespace Intellegens.Commons.Tests.Entity2DtoSearchServiceTests.Setup
{
    internal class AutomapperConfig
    {
        internal static MapperConfiguration MapperConfiguration { get; private set; }
        internal static IMapper Mapper { get; private set; }

        static AutomapperConfig()
        {
            InitializeAutoMapper();
        }

        private static void InitializeAutoMapper()
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new SearchTestEntityProfile());
            });

            MapperConfiguration = config;
            Mapper = MapperConfiguration.CreateMapper();
        }
    }
}