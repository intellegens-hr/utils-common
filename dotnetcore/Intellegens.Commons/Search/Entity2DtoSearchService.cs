using AutoMapper;
using AutoMapper.QueryableExtensions;
using Intellegens.Commons.Types;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellegens.Commons.Search
{
    /// <summary>
    /// This service uses filters based on TDto to filter IQueryable<TEntity> and always return TDto objects (single or list, ...)
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    public class Entity2DtoSearchService<TEntity, TDto>
        where TEntity : class
        where TDto : class
    {
        private readonly GenericSearchService<TEntity> searchService = new GenericSearchService<TEntity>();
        private readonly IMapper mapper;

        public Entity2DtoSearchService(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public Entity2DtoSearchService(IGenericSearchConfig genericSearchConfig, IMapper mapper) : this(mapper)
        {
            searchService = new GenericSearchService<TEntity>(genericSearchConfig);
        }

        private string TranslateDtoToEntityPath(string dtoPath)
        {
            var segments = dtoPath.Split('.').ToList();
            var newSegments = new List<string>();
            var sourceType = typeof(TDto);
            var destinationType = typeof(TEntity);

            while (segments.Any())
            {
                var segment = segments[0];
                segments.RemoveAt(0);
                var isLastElement = !segments.Any();

                var mapping = mapper.ConfigurationProvider.FindTypeMapFor(sourceType, destinationType);

                var propertyMap = mapping.PropertyMaps
                    .FirstOrDefault(pm =>
                        pm.SourceMember.Name.Equals(segment, StringComparison.OrdinalIgnoreCase)
                    );

                newSegments.Add(propertyMap.DestinationName);

                if (!isLastElement)
                {
                    sourceType = propertyMap.SourceType.TransformToUnderlyingEnumerableTypeIfExists();
                    destinationType = propertyMap.DestinationType.TransformToUnderlyingEnumerableTypeIfExists();
                }
            }

            return string.Join('.', newSegments);
        }

        private List<string> TranslateDtoToEntityPath(List<string> dtoPaths)
            => dtoPaths
                .Select(x => TranslateDtoToEntityPath(x))
                .ToList();

        /// <summary>
        ///
        /// </summary>
        /// <param name="dtoSearchRequest"></param>
        /// <returns></returns>
        private SearchRequest TranslateDtoRequestToEntityRequest(SearchRequest dtoSearchRequest)
        {
            var map = mapper.ConfigurationProvider.FindTypeMapFor<TDto, TEntity>();
            var searchRequestMapped = new SearchRequest
            {
                Offset = dtoSearchRequest.Offset,
                Limit = dtoSearchRequest.Limit,
                Filters = dtoSearchRequest
                            .Filters
                            .Select(x => new SearchFilter
                            {
                                Keys = TranslateDtoToEntityPath(x.Keys),
                                NegateExpression = x.NegateExpression,
                                Operator = x.Operator,
                                Values = x.Values
                            })
                            .ToList(),
                Search = dtoSearchRequest
                            .Search
                            .Select(x => new SearchFilter
                            {
                                Keys = TranslateDtoToEntityPath(x.Keys),
                                NegateExpression = x.NegateExpression,
                                Operator = x.Operator,
                                Values = x.Values
                            })
                            .ToList(),
                Ordering = dtoSearchRequest
                            .Ordering
                            .Select(x => new SearchOrder
                            {
                                Ascending = x.Ascending,
                                Key = TranslateDtoToEntityPath(x.Key)
                            })
                            .ToList()
            };

            return searchRequestMapped;
        }

        public async Task<List<TDto>> Search(IQueryable<TEntity> sourceData, SearchRequest searchRequest)
        {
            var searchRequestTranslated = TranslateDtoRequestToEntityRequest(searchRequest);

            return await searchService
                .FilterQuery(sourceData, searchRequestTranslated)
                .ProjectTo<TDto>(mapper.ConfigurationProvider)
                .Skip(searchRequest.Offset)
                .Take(searchRequest.Limit)
                .ToListAsync();
        }

        public async Task<(int count, List<TDto> data)> SearchAndCount(IQueryable<TEntity> sourceData, SearchRequest searchRequest)
        {
            var count = await searchService.FilterQuery(sourceData, searchRequest).CountAsync();
            var data = await Search(sourceData, searchRequest);

            return (count, data);
        }

        public async Task<int> IndexOf(string keyColumn, IQueryable<TEntity> sourceData, TDto dto, SearchRequest searchRequest)
        {
            var searchRequestTranslated = TranslateDtoRequestToEntityRequest(searchRequest);
            var entity = mapper.Map<TEntity>(dto);
            return await searchService.IndexOf(keyColumn, sourceData, entity, searchRequestTranslated);
        }
    }
}