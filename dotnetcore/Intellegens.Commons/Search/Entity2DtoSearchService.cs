﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Intellegens.Commons.Search.FullTextSearch;
using Intellegens.Commons.Search.Models;
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
        where TEntity : class, new()
        where TDto : class, new()
    {
        private readonly GenericSearchService<TEntity> searchService = new GenericSearchService<TEntity>();
        private readonly IMapper mapper;

        public Entity2DtoSearchService(IMapper mapper)
        {
            this.mapper = mapper;
            InitFullTextSearchPathTranslation();
        }

        public Entity2DtoSearchService(IGenericSearchConfig genericSearchConfig, IMapper mapper)
        {
            searchService = new GenericSearchService<TEntity>(genericSearchConfig);
            this.mapper = mapper;
            InitFullTextSearchPathTranslation();
        }

        private void InitFullTextSearchPathTranslation()
        {
            if (searchService.FullTextSearchPaths.Any())
                searchService.FullTextSearchPaths = TranslateDtoToEntityPath(FullTextSearchExtensions.GetFullTextSearchPaths<TDto>());
        }

        /// <summary>
        /// Take string path for DTO (e.g. customerDtos.parentDto.id) to entity path (customers.parent.id).
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

            while (segments.Any())
            {
                var segment = segments[0];
                segments.RemoveAt(0);
                var isLastElement = !segments.Any();

                var mapping = mapper.ConfigurationProvider.FindTypeMapFor(sourceType, destinationType);

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

        private List<string> TranslateDtoToEntityPath(List<string> dtoPaths)
            => (dtoPaths ?? new List<string>())
                .Select(x => TranslateDtoToEntityPath(x))
                .ToList();

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
                Criteria = searchCriteria.Criteria?.Select(x => TranslateSearchCriteriaFromDtoToEntity(x)).ToList() ?? new List<SearchCriteria>(),
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

                Keys = dtoSearchRequest.Keys.Select(x => TranslateDtoToEntityPath(x)).ToList(),
                KeysLogic = dtoSearchRequest.KeysLogic,
                Values = dtoSearchRequest.Values,
                ValuesLogic = dtoSearchRequest.ValuesLogic,

                Operator = dtoSearchRequest.Operator,
                Negate = dtoSearchRequest.Negate,

                Criteria = dtoSearchRequest
                            .Criteria
                            .Select(x => TranslateSearchCriteriaFromDtoToEntity(x))
                            .ToList(),

                CriteriaLogic = dtoSearchRequest.CriteriaLogic,

                Order = dtoSearchRequest
                            .Order
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