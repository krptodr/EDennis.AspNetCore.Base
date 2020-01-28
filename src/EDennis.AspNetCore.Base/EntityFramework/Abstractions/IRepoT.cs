﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace EDennis.AspNetCore.Base.EntityFramework {
    public interface IRepo<TEntity, TContext> : IRepo
        where TEntity : class, IHasSysUser, new()
        where TContext : DbContext {
        TContext Context { get; set; }
        IQueryable<TEntity> Query { get; }
        IScopeProperties ScopeProperties { get; set; }

        TEntity Create(TEntity entity);
        Task<TEntity> CreateAsync(TEntity entity);
        void Delete(params object[] keyValues);
        Task DeleteAsync(params object[] keyValues);
        bool Exists(params object[] keyValues);
        Task<bool> ExistsAsync(params object[] keyValues);
        TEntity Get(params object[] keyValues);
        Task<TEntity> GetAsync(params object[] keyValues);
        DynamicLinqResult GetWithDynamicLinq(string select, string where = null, string orderBy = null, int? skip = null, int? take = null, int? totalRecords = null);
        Task<DynamicLinqResult> GetWithDynamicLinqAsync(string select, string where = null, string orderBy = null, int? skip = null, int? take = null, int? totalRecords = null);
        DynamicLinqResult<TEntity> GetWithDynamicLinq(string where = null, string orderBy = null, int? skip = null, int? take = null, int? totalRecords = null);
        Task<DynamicLinqResult<TEntity>> GetWithDynamicLinqAsync(string where = null, string orderBy = null, int? skip = null, int? take = null, int? totalRecords = null);
        TEntity Patch(dynamic partialEntity, params object[] keyValues);
        TEntity Update(TEntity entity, params object[] keyValues);
        Task<TEntity> PatchAsync(dynamic partialEntity, params object[] keyValues);
        Task<TEntity> UpdateAsync(TEntity entity, params object[] keyValues);
    }
}