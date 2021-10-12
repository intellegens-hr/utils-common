﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Intellegens.Commons.Search.FullTextSearch;
using Intellegens.Commons.Search.Models;
using Intellegens.Commons.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellegens.Commons.Search
{
    /// <summary>
    /// This service uses filters based on TDto to filter IQueryable<TEntity> and always return TDto objects (single or list, ...)
    ///
    /// To avoid all issues when filtering/ordering IQueryables which are AutoMapped from entity to some dto, this service uses
    /// SearchRequest made for TDto, translates it, filters IQueryable<TEntity> and maps it to TDto after doing all EF operations
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    public class Entity2DtoSearchService<TEntity, TDto> : IEntity2DtoSearchService<TEntity, TDto> where TEntity : class, new()
        where TDto : class, new()
    {
        private readonly IGenericSearchService<TEntity> searchService;
        private readonly IMapper mapper;

        public Entity2DtoSearchService(IMapper mapper, IGenericSearchService<TEntity> searchService)
        {
            this.mapper = mapper;
            this.searchService = searchService;
            InitFullTextSearchPathTranslation();
        }

        private void InitFullTextSearchPathTranslation()
        {
            if (searchService.FullTextSearchPaths.Any())
                searchService.FullTextSearchPaths = TranslateDtoToEntityPath(FullTextSearchExtensions.GetFullTextSearchPaths<TDto>());
        }

        /// <summary>
        /// Take string path for DTO (e.g. customerDtos.parentDto.id) and map it to entity path (customers.parent.id).
        /// Uses Automapper to translate paths
        /// </summary>
        /// <param name="dtoPath"></param>
        /// <returns></returns>
        private string TranslateDtoToEntityPath(string dtoPath)
        {
            var segments = dtoPath.Split('.').ToList();
            var newSegments = new List<string>();
            var sourceType = typeof(TDto);
            var destinationType = typeof(TEntity);

            // go through all segments and map to entity
            while (segments.Any())
            {
                var segment = segments[0];
                segments.RemoveAt(0);
                var isLastElement = !segments.Any();

                // use automapper to find target property
                var mapping = mapper.ConfigurationProvider.FindTypeMapFor(sourceType, destinationType);

                if (mapping?.PropertyMaps == null)
                {
                    throw new Exception($"Missing mappings for: {sourceType.Name} -> {destinationType.Name}");
                }

                var propertyMap = mapping.PropertyMaps
                    .Where(pm => pm.SourceMember != null)
                    .FirstOrDefault(pm =>
                        pm.SourceMember.Name.Equals(segment, StringComparison.OrdinalIgnoreCase)
                    );

                if (propertyMap == null)
                    continue;

                newSegments.Add(propertyMap.DestinationName);

                if (!isLastElement)
                {
                    sourceType = propertyMap.SourceType.TransformToUnderlyingEnumerableTypeIfExists();
                    destinationType = propertyMap.DestinationType.TransformToUnderlyingEnumerableTypeIfExists();
                }
            }

            return string.Join('.', newSegments);
        }

        private IEnumerable<string> TranslateDtoToEntityPath(IEnumerable<string> dtoPaths)
            => (dtoPaths ?? Enumerable.Empty<string>())
                .Select(x => TranslateDtoToEntityPath(x));

        /// <summary>
        /// Build entity criteria by translating dto criteria
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        private SearchCriteria TranslateSearchCriteriaFromDtoToEntity(SearchCriteria searchCriteria)
        {
            return new SearchCriteria
            {
                Keys = TranslateDtoToEntityPath(searchCriteria.Keys),
                KeysLogic = searchCriteria.KeysLogic,
                Negate = searchCriteria.Negate,
                Operator = searchCriteria.Operator,
                Values = searchCriteria.Values,
                ValuesLogic = searchCriteria.ValuesLogic,
                Criteria = searchCriteria.Criteria?.Select(x => TranslateSearchCriteriaFromDtoToEntity(x)) ?? Enumerable.Empty<SearchCriteria>(),
                CriteriaLogic = searchCriteria.CriteriaLogic
            };
        }

        /// <summary>
        /// Translate all filter/search/order paths specified on DTO and translate them to entity paths
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

                Keys = dtoSearchRequest.Keys.Select(x => TranslateDtoToEntityPath(x)),
                KeysLogic = dtoSearchRequest.KeysLogic,
                Values = dtoSearchRequest.Values,
                ValuesLogic = dtoSearchRequest.ValuesLogic,

                Operator = dtoSearchRequest.Operator,
                Negate = dtoSearchRequest.Negate,

                Criteria = dtoSearchRequest
                            .Criteria
                            .Select(x => TranslateSearchCriteriaFromDtoToEntity(x)),

                CriteriaLogic = dtoSearchRequest.CriteriaLogic,

                Order = dtoSearchRequest
                            .Order
                            .Select(x => new SearchOrder
                            {
                                Ascending = x.Ascending,
                                Key = TranslateDtoToEntityPath(x.Key)
                            }),
                OrderByMatchCount = dtoSearchRequest.OrderByMatchCount
            };

            return searchRequestMapped;
        }

        /// <summary>
        /// Same as SearchService method but uses IQueryable<TEntity> to do all EF operations and maps it to TDto
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        public async Task<List<TDto>> Search(IQueryable<TEntity> sourceData, SearchRequest searchRequest)
        {
            var searchRequestTranslated = TranslateDtoRequestToEntityRequest(searchRequest);

            return await searchService
                .FilterQuery(sourceData, searchRequestTranslated)
                .Skip(searchRequest.Offset)
                .Take(searchRequest.Limit)
                .ProjectTo<TDto>(mapper.ConfigurationProvider)
                .ToListAsync();
        }

        /// <summary>
        /// Same as SearchService method - return count and data for given request
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        public async Task<(int count, List<TDto> data)> SearchAndCount(IQueryable<TEntity> sourceData, SearchRequest searchRequest)
        {
            var searchRequestTranslated = TranslateDtoRequestToEntityRequest(searchRequest);
            var count = await searchService.FilterQuery(sourceData, searchRequestTranslated).CountAsync();

            var data = await Search(sourceData, searchRequest);

            return (count, data);
        }

        /// <summary>
        /// Same as SearchService method - find index of given ID in result set
        /// </summary>
        /// <param name="keyColumn"></param>
        /// <param name="sourceData"></param>
        /// <param name="dto"></param>
        /// <param name="searchRequest"></param>
        /// <returns></returns>
        public async Task<int> IndexOf(string keyColumn, IQueryable<TEntity> sourceData, TDto dto, SearchRequest searchRequest)
        {
            var searchRequestTranslated = TranslateDtoRequestToEntityRequest(searchRequest);
            var entity = mapper.Map<TEntity>(dto);
            return await searchService.IndexOf(keyColumn, sourceData, entity, searchRequestTranslated);
        }
    }
}