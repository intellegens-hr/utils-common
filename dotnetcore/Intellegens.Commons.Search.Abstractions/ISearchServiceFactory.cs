namespace Intellegens.Commons.Search
{
    public interface ISearchServiceFactory
    {
        public IEntity2DtoSearchService<TEntity, TDto> GetEntity2DtoSearchService<TEntity, TDto>()
            where TEntity : class, new()
            where TDto : class, new();

        public IGenericSearchService<T> GetSearchService<T>()
            where T : class, new();
    }
}