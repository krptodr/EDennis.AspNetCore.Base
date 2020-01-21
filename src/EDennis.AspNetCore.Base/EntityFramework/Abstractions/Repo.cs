﻿using EDennis.AspNetCore.Base.Logging;
using EDennis.AspNetCore.Base.Serialization;
using EDennis.AspNetCore.Base.Web;
using MethodBoundaryAspect.Fody.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace EDennis.AspNetCore.Base.EntityFramework {

    /// <summary>
    /// A read/write base repository class, backed by
    /// a DbSet, exposing basic CRUD methods, as well
    /// as methods exposed by QueryableRepo 
    /// </summary>
    /// <typeparam name="TEntity">The associated model class</typeparam>
    /// <typeparam name="TContext">The associated DbContextBase class</typeparam>
    [ScopedTraceLogger(logEntry: true)]
    [AspectSkipProperties(true)]
    public partial class Repo<TEntity, TContext> : IRepo, IRepo<TEntity, TContext>
        where TEntity : class, IHasSysUser, new()
        where TContext : DbContext {

        [DisableWeaving] public virtual string GetScopedTraceLoggerKey() => ScopeProperties?.ScopedTraceLoggerKey;

        public TContext Context { get; set; }
        public IScopeProperties ScopeProperties { get; set; }

        public ILogger Logger { get; }

        /// <summary>
        /// Constructs a new RepoBase object using the provided DbContext
        /// </summary>
        /// <param name="context">Entity Framework DbContext</param>
        public Repo(DbContextProvider<TContext> provider,
            IScopeProperties scopeProperties,
            ILogger<Repo<TEntity, TContext>> logger) {

            Context = provider.Context;
            ScopeProperties = scopeProperties;
            Logger = logger;

        }



        /// <summary>
        /// Retrieves the entity with the provided primary key values
        /// </summary>
        /// <param name="keyValues">primary key provided as key-value object array</param>
        /// <returns>Entity whose primary key matches the provided input</returns>
        public virtual TEntity GetById(params object[] keyValues) {
            var entity = Context.Find<TEntity>(keyValues);
            if (entity != null)
                Context.Entry(entity).State = EntityState.Detached;
            return entity;
        }


        /// <summary>
        /// Asychronously retrieves the entity with the provided primary key values.
        /// </summary>
        /// <param name="keyValues">primary key provided as key-value object array</param>
        /// <returns>Entity whose primary key matches the provided input</returns>

        public virtual async Task<TEntity> GetByIdAsync(params object[] keyValues) {
            var entity = await Context.FindAsync<TEntity>(keyValues);
            if (entity != null)
                Context.Entry(entity).State = EntityState.Detached;
            return entity;
        }



        /// <summary>
        /// Get by Dynamic Linq Expression
        /// https://github.com/StefH/System.Linq.Dynamic.Core
        /// https://github.com/StefH/System.Linq.Dynamic.Core/wiki/Dynamic-Expressions
        /// </summary>
        /// <param name="where">string Where expression</param>
        /// <param name="orderBy">string OrderBy expression (with support for descending)</param>
        /// <param name="select">string Select expression</param>
        /// <param name="skip">int number of records to skip</param>
        /// <param name="take">int number of records to return</param>
        /// <returns>dynamic-typed object</returns>

        public virtual PagedResult<dynamic> GetFromDynamicLinq(
                string where = null,
                string orderBy = null,
                string select = null,
                int? skip = null,
                int? take = null,
                int? totalRecords = null) {

            IQueryable qry = BuildLinqQuery(where, orderBy, select, skip, take, totalRecords, 
                out PagingData pagingData);

            var result = qry.ToDynamicList();
            return new PagedResult<dynamic> {
                Data = result,
                PagingData = pagingData
            };

        }





        /// <summary>
        /// Get by Dynamic Linq Expression
        /// https://github.com/StefH/System.Linq.Dynamic.Core
        /// https://github.com/StefH/System.Linq.Dynamic.Core/wiki/Dynamic-Expressions
        /// </summary>
        /// <param name="where">string Where expression</param>
        /// <param name="orderBy">string OrderBy expression (with support for descending)</param>
        /// <param name="select">string Select expression</param>
        /// <param name="skip">int number of records to skip</param>
        /// <param name="take">int number of records to return</param>
        /// <returns>dynamic-typed object</returns>

        public virtual async Task<PagedResult<dynamic>> GetFromDynamicLinqAsync(
                string where = null,
                string orderBy = null,
                string select = null,
                int? skip = null,
                int? take = null,
                int? totalRecords = null) {

            IQueryable qry = BuildLinqQuery(where, orderBy, select, skip, take, totalRecords, out PagingData pagingData);
            var result = await qry.ToDynamicListAsync();

            return new PagedResult<dynamic> {
                Data = result,
                PagingData = pagingData
            };

        }


        private IQueryable BuildLinqQuery(string where, string orderBy, string select, int? skip, int? take, int? totalRecords, out PagingData pagingData) {

            IQueryable qry = Query;

            if (where != null)
                qry = qry.Where(where);
            if (orderBy != null)
                qry = qry.OrderBy(orderBy);
            if (select != null)
                qry = qry.Select(select);

            if (totalRecords == null || totalRecords.Value < 0)
                totalRecords = qry.Count();

            var skipValue = skip == null ? 0 : skip.Value;
            var takeValue = take == null ? totalRecords.Value - skipValue : take.Value;
            var pageCount = (int)Math.Ceiling(totalRecords.Value / (double)takeValue);

            pagingData = new PagingData {
                RecordCount = totalRecords.Value,
                PageSize = takeValue,
                PageNumber = 1+ (int)Math.Ceiling((skipValue) /(double)takeValue),
                PageCount = pageCount
            };
            if (skipValue != 0)
                qry = qry.Skip(skipValue);
            if (take != null && take.Value > 0)
                qry = qry.Take(takeValue);

            return qry;
        }


        /// <summary>
        /// Provides direct access to the Query property of the context,
        /// allowing any query to be constructed via Linq expression
        /// </summary>

        public virtual IQueryable<TEntity> Query { get => Context.Set<TEntity>().AsNoTracking(); }


        /// <summary>
        /// Determines if an object with the given primary key values
        /// exists in the context.
        /// </summary>
        /// <param name="keyValues">primary key values</param>
        /// <returns>true if an entity with the provided keys exists</returns>

        public virtual bool Exists(params object[] keyValues) {
            var entity = Context.Find<TEntity>(keyValues);
            if (entity != null)
                Context.Entry(entity).State = EntityState.Detached;
            var exists = (entity != null);
            return exists;
        }


        /// <summary>
        /// Determines if an object with the given primary key values
        /// exists in the context.
        /// </summary>
        /// <param name="keyValues">primary key values</param>
        /// <returns></returns>

        public virtual async Task<bool> ExistsAsync(params object[] keyValues) {
            var entity = await Context.FindAsync<TEntity>(keyValues);
            if (entity != null)
                Context.Entry(entity).State = EntityState.Detached;
            var exists = (entity != null);
            return exists;
        }

        /// <summary>
        /// Creates a new entity from the provided input
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>The created entity</returns>

        public virtual TEntity Create(TEntity entity) {
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot create a null {entity.GetType().Name}");

            SetSysUser(entity);
            Context.Add(entity);
            Context.SaveChanges();
            return entity;
        }

        /// <summary>
        /// Asynchronously creates a new entity from the provided input
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>The created entity</returns>

        public virtual async Task<TEntity> CreateAsync(TEntity entity) {
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot create a null {entity.GetType().Name}");

            SetSysUser(entity);

            Context.Add(entity);
            await Context.SaveChangesAsync();
            return entity;
        }




        /// <summary>
        /// Updates the provided entity
        /// </summary>
        /// <param name="entity">The new data for the entity</param>
        /// <returns>The newly updated entity</returns>

        public virtual TEntity Update(TEntity entity, params object[] keyValues) {
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot update a null {entity.GetType().Name}");

            //retrieve the existing entity
            var existing = Context.Find<TEntity>(keyValues);

            SetSysUser(entity);

            //copy property values from entity to existing (resultEntity)
            Context.Entry(existing).CurrentValues.SetValues(entity);
            Context.Entry(existing).State = EntityState.Detached;

            Context.Update(entity);
            Context.SaveChanges();
            return existing;
        }




        /// <summary>
        /// Asynchronously updates the provided entity
        /// </summary>
        /// <param name="entity">The new data for the entity</param>
        /// <returns>The newly updated entity</returns>

        public virtual async Task<TEntity> UpdateAsync(TEntity entity, params object[] keyValues) {

            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot update a null {entity.GetType().Name}");

            //retrieve the existing entity (resultEntity)
            var existing = await Context.FindAsync<TEntity>(keyValues);

            SetSysUser(entity);

            //copy property values from entity to existing
            Context.Entry(existing).CurrentValues.SetValues(entity);
            Context.Entry(existing).State = EntityState.Detached;

            Context.Update(entity);
            await Context.SaveChangesAsync();
            return existing;
        }




        public virtual TEntity Update(dynamic partialEntity, params object[] keyValues) {
            if (partialEntity == null)
                throw new MissingEntityException(
                    $"Cannot update a null {typeof(TEntity).Name}");


            //retrieve the existing entity
            var existing = Context.Find<TEntity>(keyValues);

            SetSysUser(partialEntity);

            //copy property values from entity to existing
            DynamicExtensions.Populate<TEntity>(existing, partialEntity);
            Context.Entry(existing).State = EntityState.Detached;


            Context.Update(existing);
            Context.SaveChanges();
            return existing; //updated entity

        }




        public virtual async Task<TEntity> UpdateAsync(dynamic partialEntity, params object[] keyValues) {
            if (partialEntity == null)
                throw new MissingEntityException(
                    $"Cannot update a null {typeof(TEntity).Name}");

            SetSysUser(partialEntity);

            //retrieve the existing entity
            var existing = await Context.FindAsync<TEntity>(keyValues);

            //copy property values from entity to existing
            DynamicExtensions.Populate<TEntity>(existing, partialEntity);
            Context.Entry(existing).State = EntityState.Detached;

            Context.Update(existing);
            await Context.SaveChangesAsync();
            return existing; //updated entity
        }








        public virtual TEntity Update(PartialEntity<TEntity> partialEntity, params object[] keyValues) {
            if (partialEntity == null)
                throw new MissingEntityException(
                    $"Cannot update a null {typeof(TEntity).Name}");


            //retrieve the existing entity
            var existing = Context.Find<TEntity>(keyValues);

            partialEntity.Entity.SysUser = ScopeProperties.User;

            //copy property values from entity to existing
            partialEntity.MergeInto(existing);
            Context.Entry(existing).State = EntityState.Detached;

            Context.Update(existing);
            Context.SaveChanges();
            return existing; //updated entity

        }




        public virtual async Task<TEntity> UpdateAsync(PartialEntity<TEntity> partialEntity, params object[] keyValues) {
            if (partialEntity == null)
                throw new MissingEntityException(
                    $"Cannot update a null {typeof(TEntity).Name}");

            partialEntity.Entity.SysUser = ScopeProperties.User;

            //retrieve the existing entity
            var existing = await Context.FindAsync<TEntity>(keyValues);

            partialEntity.Entity.SysUser = ScopeProperties.User;

            //copy property values from entity to existing
            partialEntity.MergeInto(existing);
            Context.Entry(existing).State = EntityState.Detached;

            Context.Update(existing);
            await Context.SaveChangesAsync();
            return existing; //updated entity
        }








        /// <summary>
        /// Deletes the entity whose primary keys match the provided input
        /// </summary>
        /// <param name="keyValues">The primary key as key-value object array</param>

        public virtual void Delete(params object[] keyValues) {

            var existing = Context.Find<TEntity>(keyValues);
            if (existing == null)
                throw new MissingEntityException(
                    $"Cannot find {typeof(TEntity).Name} object with key value = {PrintKeys(keyValues)}");

            Context.Remove(existing);
            Context.SaveChanges();

        }




        /// <summary>
        /// Asynchrously deletes the entity whose primary keys match the provided input
        /// </summary>
        /// <param name="keyValues">The primary key as key-value object array</param>

        public virtual async Task DeleteAsync(params object[] keyValues) {
            var existing = Context.Find<TEntity>(keyValues);
            if (existing == null)
                throw new MissingEntityException(
                    $"Cannot find {typeof(TEntity).Name} object with key value = {PrintKeys(keyValues)}");

            Context.Remove(existing);
            await Context.SaveChangesAsync();
            return;
        }


        #region helper methods
        protected void SetSysUser(dynamic entity) { entity.SysUser = ScopeProperties.User; }
        protected void SetSysUser(TEntity entity) { entity.SysUser = ScopeProperties.User; }
        protected static string PrintKeys(params object[] keyValues) {
            return "[" + string.Join(",", keyValues) + "]";
        }
        #endregion


    }


}

