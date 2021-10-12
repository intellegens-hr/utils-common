using AutoMapper;

namespace Intellegens.Commons.Search
{
    public class SearchServiceFactory : ISearchServiceFactory
    {
        private readonly IMapper mapper;

        public SearchServiceFactory(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public IEntity2DtoSearchService<TEntity, TDto> GetEntity2DtoSearchService<TEntity, TDto>()
            where TEntity : class, new()
            where TDto : class, new()
        {
            return new Entity2DtoSearchService<TEntity, TDto>(mapper, GetSearchService<TEntity>());
        }

        public IGenericSearchService<T> GetSearchService<T>()
            where T : class, new()
        {
            return new GenericSearchService<T>();
        }
    }
}