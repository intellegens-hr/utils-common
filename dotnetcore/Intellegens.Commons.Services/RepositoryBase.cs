using AutoMapper;
using AutoMapper.QueryableExtensions;
using Intellegens.Commons.Results;
using Intellegens.Commons.Search;
using Intellegens.Commons.Search.Models;
using Intellegens.Commons.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Intellegens.Commons.Services
{
    public abstract class RepositoryBase<TEntity, TDto> : RepositoryBase<TEntity, TDto, int>, IRepositoryBase<TDto>
            where TDto : class, IDtoBase<int>, new()
            where TEntity : class, new()
    {
        public RepositoryBase(DbContext dbContext, IMapper mapper, ISearchServiceFactory searchServiceFactory) : base(dbContext, mapper, searchServiceFactory)
        {
        }
    }

    public abstract class RepositoryBase<TEntity, TDto, TKey> : IRepositoryBase<TDto, TKey>
            where TDto : class, IDtoBase<TKey>, new()
            where TEntity : class, new()
    {
        protected readonly DbContext dbContext;
        protected readonly DbSet<TEntity> dbSet;

        protected readonly IMapper mapper;

        protected readonly IEntity2DtoSearchService<TEntity, TDto> searchService;

        public RepositoryBase(DbContext dbContext, IMapper mapper, ISearchServiceFactory searchServiceFactory)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.searchService = searchServiceFactory.GetEntity2DtoSearchService<TEntity, TDto>();
            dbSet = dbContext.Set<TEntity>();
        }

        public virtual async Task<Result<IEnumerable<TDto>>> All()
        {
            var data = await GetDtoQueryable().ToListAsync();

            var postProcessedData = data.Select(x => FetchPostProcess(x));
            return Result<IEnumerable<TDto>>.SuccessDataResult(postProcessedData);
        }

        public virtual async Task<Result<TDto>> Create(TEntity entity)
        {
            if (entity is IValidatableObject entityValidatable)
            {
                var validationMessages = entityValidatable.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(entity));
                if (validationMessages.Any())
                    return Result<TDto>.ErrorDataResult(validationMessages.First().ErrorMessage);
            }

            var validationResultsDb = DataAnnotationsValidationUtil.ValidateDataAnnotations(entity);
            if (validationResultsDb.Any())
            {
                return Result<TDto>.ErrorDataResult(CommonErrorCodes.ValidationError);
            }

            dbSet.Add(entity);

            await dbContext.SaveChangesAsync();

            var dtoMapped = mapper.Map<TDto>(entity);

            return await FindById(dtoMapped.GetIdValue());
        }

        public virtual async Task<Result<TDto>> Create(TDto entityDto)
        {
            var validationResultsDto = DataAnnotationsValidationUtil.ValidateDataAnnotations(entityDto);
            if (validationResultsDto.Any())
            {
                return Result<TDto>.ErrorDataResult(CommonErrorCodes.ValidationError);
            }

            var entity = mapper.Map<TEntity>(entityDto);

            return await this.Create(entity);
        }

        public virtual async Task<Result> Delete(TKey id)
        {
            var entity = await FindEntityById(id);

            if (entity == null)
                return Result.ErrorResult(CommonErrorCodes.NotFound);

            dbContext.Remove(entity);

            await dbContext.SaveChangesAsync();
            return Result.SuccessResult();
        }

        public virtual async Task<Result> Delete(TDto entityDto)
        {
            return await Delete(entityDto.GetIdValue());
        }

        public virtual async Task<Result<TDto>> FindById(TKey id)
        {
            var dto = await GetDtoQueryableById(id).FirstAsync();

            if (dto == null)
                return Result<TDto>.ErrorDataResult(CommonErrorCodes.NotFound);

            return Result<TDto>.SuccessDataResult(FetchPostProcess(dto));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dtoQuery"></param>
        /// <param name="searchRequest"></param>
        /// <param name="id">Entity ID</param>
        /// <param name="secondarySortColumn">Secondary sort parameter - if omitted, ID property will be used</param>
        /// <returns></returns>
        public virtual async Task<Result<int>> IndexOf(SearchRequest searchRequest, TKey id, string secondarySortColumn = null, Expression<Func<TEntity, bool>> entityFilter = null)
        {
            var dto = await FindById(id);

            if (!dto.Success)
                return Result<int>.ErrorDataResult(dto.Errors.First());

            var dtoData = dto.Data;
            secondarySortColumn ??= dtoData.GetIdPropertyName();
            var index = await searchService.IndexOf(secondarySortColumn, GetEntityQueryable(entityFilter), dtoData, searchRequest);
            return Result<int>.SuccessDataResult(index);
        }

        public virtual async Task<Result<int>> IndexOf(SearchRequest searchRequest, TDto dto, string secondarySortColumn = null, Expression<Func<TEntity, bool>> entityFilter = null)
        {
            secondarySortColumn ??= dto.GetIdPropertyName();
            var index = await searchService.IndexOf(secondarySortColumn, GetEntityQueryable(entityFilter), dto, searchRequest);
            return Result<int>.SuccessDataResult(index);
        }

        public Task<Result<int>> IndexOf(SearchRequest searchRequest, TKey id)
        {
            return IndexOf(searchRequest, id, null);
        }

        public virtual async Task<(int? count, IEnumerable<TDto> data)> Search(SearchRequest searchRequest, bool calculateTotalRecordCount = true, Expression<Func<TEntity, bool>> entityFilter = null)
        {
            int? count = null;
            var data = new List<TDto>();

            if (calculateTotalRecordCount)
            {
                var result = await searchService.SearchAndCount(GetEntityQueryable(entityFilter), searchRequest);
                count = result.count;
                data = result.data;
            }
            else
            {
                data = await searchService.Search(GetEntityQueryable(entityFilter), searchRequest);
            }

            var postProcessedData = data.Select(x => FetchPostProcess(x));
            return (count, postProcessedData);
        }

        public Task<(int? count, IEnumerable<TDto> data)> Search(SearchRequest searchRequest, bool calculateTotalRecordCount = true)
        {
            return Search(searchRequest, calculateTotalRecordCount, null);
        }

        public virtual async Task<Result<TDto>> Update(TDto entityDto)
        {
            // if dto implements IValidatable - try to validate it
            if (entityDto is IValidatableObject dtoValidatable)
            {
                var validationMessages = dtoValidatable.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(entityDto));
                if (validationMessages.Any())
                    return Result<TDto>.ErrorDataResult(validationMessages.First().ErrorMessage);
            }

            //find entity in datbase, map it and validate
            var entity = await FindEntityById(entityDto.GetIdValue());
            if (entity == null)
                return Result<TDto>.ErrorDataResult(CommonErrorCodes.NotFound);

            mapper.Map(entityDto, entity);

            if (entity is IValidatableObject entityValidatable)
            {
                var validationMessages = entityValidatable.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(entity));
                if (validationMessages.Any())
                    return Result<TDto>.ErrorDataResult(validationMessages.First().ErrorMessage);
            }

            var validationResultsDb = DataAnnotationsValidationUtil.ValidateDataAnnotations(entity);
            if (validationResultsDb.Any())
            {
                return Result<TDto>.ErrorDataResult(CommonErrorCodes.ValidationError);
            }

            // save changes and return
            await dbContext.SaveChangesAsync();
            return await FindById(entityDto.GetIdValue());
        }

        /// <summary>
        /// Any repository method which returns DTO will use this method on each element before returning
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        protected virtual TDto FetchPostProcess(TDto dto)
        {
            return dto;
        }

        protected virtual async Task<TEntity> FindEntityById(TKey id)
                    => await dbSet.FindAsync(id);

        /// <summary>
        /// By default, AutoMapper is used, override this to specify IQueryable<TDto>
        /// </summary>
        /// <returns></returns>
        protected virtual IQueryable<TDto> GetDtoQueryable()
        {
            return dbSet
                .ProjectTo<TDto>(mapper.ConfigurationProvider)
                .AsQueryable();
        }

        protected virtual IQueryable<TEntity> GetEntityQueryable(Expression<Func<TEntity, bool>> entityFilter)
        {
            var query = dbSet.AsQueryable();

            if (entityFilter != null)
                query = query.Where(entityFilter);

            return query;
        }

        private IQueryable<TDto> GetDtoQueryableById(TKey id)
        {
            return GetDtoQueryable()
                .Where($"{new TDto().GetIdPropertyName()} == @0", id);
        }
    }
}