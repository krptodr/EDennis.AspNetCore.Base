﻿using DevExtreme.AspNet.Data;
using EDennis.AspNetCore.Base.EntityFramework;
using EDennis.AspNetCore.Base.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.AspNetCore.Base.Web {

    [ApiController]
    [Route("api/[controller]")]
    public abstract class RepoController<TEntity, TContext, TRepo> : ControllerBase 
        where TEntity: class, IHasSysUser, new()
        where TContext: DbContext
        where TRepo : IRepo<TEntity,TContext> {

        public TRepo Repo { get; set; }

        public static PropertyInfo[] Properties;
        public static Func<string, object[]> ParseId;
        public static Func<dynamic, object[]> GetPrimaryKeyDynamic;
        public static Func<TEntity, object[]> GetPrimaryKey;
        public static IReadOnlyList<IProperty> KeyProperties;

        static RepoController() {

            Properties = typeof(TEntity).GetProperties();

            ParseId = (s) => {
                var key = s.Split('~');
                var id = new object[KeyProperties.Count];
                for (int i = 0; i < id.Length; i++)
                    id[i] = Convert.ChangeType(key[i],KeyProperties[i].ClrType);
                return id;
            };

            GetPrimaryKeyDynamic = (dyn) => {
                var id = new object[KeyProperties.Count];
                Type dynType = dyn.GetType();
                PropertyInfo[] props = dynType.GetProperties();
                for (int i = 0; i < KeyProperties.Count; i++) {
                    var keyName = KeyProperties[i].Name;
                    var keyType = KeyProperties[i].ClrType;
                    var prop = props.FirstOrDefault(p => p.Name == keyName);
                    if (prop == null)
                        throw new ArgumentException($"The provided input does not contain a {keyName} property");
                    var keyValue = prop.GetValue(dyn);
                    id[i] = Convert.ChangeType(keyValue,keyType);
                }
                return id;
            };

            GetPrimaryKey = (entity) => {
                var id = new object[KeyProperties.Count];
                for (int i = 0; i < KeyProperties.Count; i++) {
                    var keyName = KeyProperties[i].Name;
                    var keyType = KeyProperties[i].ClrType;
                    var prop = Properties.FirstOrDefault(p => p.Name == keyName);
                    var keyValue = prop.GetValue(entity);
                    id[i] = Convert.ChangeType(keyValue, keyType);
                }
                return id;
            };


        }


        const string IDREGEX = "{id:regex(((?:[[^~]]+~[[^~]]+(?:~[[^~]]+)*)|(?:^-?[[0-9]]+$)))}";
        const string ASYNC_IDREGEX = "async/{id:regex(((?:[[^~]]+~[[^~]]+(?:~[[^~]]+)*)|(?:^-?[[0-9]]+$)))}";

        public JsonSerializerOptions JsonSerializationOptions { get; set; }

        public RepoController(TRepo repo) {
            Repo = repo;
            if (KeyProperties == null)
                KeyProperties = Repo.Context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties;

            JsonSerializationOptions = new JsonSerializerOptions();
            JsonSerializationOptions.Converters.Add(new DynamicJsonConverter<TEntity>());
        }


        [HttpGet(IDREGEX)]
        public virtual IActionResult GetById([FromRoute]string id) {
            var entity = Repo.GetById(ParseId(id));
            if (entity == null)
                return NotFound();
            else
                return Ok(entity);
        }


        [HttpGet(ASYNC_IDREGEX)]
        public virtual async Task<IActionResult> GetByIdAsync([FromRoute]string id) {
            var entity = await Repo.GetByIdAsync(ParseId(id));
            if (entity == null)
                return NotFound();
            else
                return Ok(entity);
        }

        [HttpPost]
        public virtual IActionResult Create([FromBody]TEntity entity) {
            var pk = GetPrimaryKey(entity);
            try {
                var created = Repo.Create(entity);
                return Ok(created);
            } catch (DbUpdateException) {
                if (Repo.Exists(pk)) {
                    ModelState.AddModelError("", $"A {typeof(TEntity).Name} instance with the specified id {pk.ToTildaDelimited()} already exists");
                    return Conflict(ModelState);
                } else {
                    throw;
                }
            }
            //return CreatedAtAction("GetById", new { id = pk.ToTildaDelimited() }, entity);
        }

        [HttpPost("async")]
        public virtual async Task<IActionResult> CreateAsync([FromBody] TEntity entity) {
            var pk = GetPrimaryKey(entity);
            try {
                var created = await Repo.CreateAsync(entity);
                return Ok(created);
            } catch (DbUpdateException) {
                if (Repo.Exists(pk)) {
                    ModelState.AddModelError("", $"A {typeof(TEntity).Name} instance with the specified id {pk.ToTildaDelimited()} already exists");
                    return Conflict(ModelState);
                } else {
                    throw;
                }
            }
            //return CreatedAtAction("GetById", new { id = pk.ToTildaDelimited() }, entity);
        }


        [HttpPut(IDREGEX)]
        public virtual IActionResult Update([FromBody]TEntity entity, [FromRoute]string id) {
            var ePk = GetPrimaryKey(entity);
            var iPk = ParseId(id);

            if (!ePk.EqualsAll(iPk))
                return BadRequest($"The path parameter id ({id}) does not match the provided object's id ({ePk.ToTildaDelimited()})");

            try {
                var updated = Repo.Update(entity,iPk);
                return Ok(updated);
            } catch (DbUpdateConcurrencyException) {
                if (!Repo.Exists(iPk))
                    return NotFound();
                else
                    throw;
            }
        }


        [HttpPut(ASYNC_IDREGEX)]
        public virtual async Task<IActionResult> UpdateAsync([FromBody]TEntity entity, [FromRoute]string id) {
            var ePk = GetPrimaryKey(entity);
            var iPk = ParseId(id);

            if (!ePk.EqualsAll(iPk))
                return BadRequest($"The path parameter id ({id}) does not match the provided object's id ({ePk.ToTildaDelimited()})");

            try {
                var updated = await Repo.UpdateAsync(entity, iPk);
                return Ok(updated);
            } catch (DbUpdateConcurrencyException) {
                if (!Repo.Exists(iPk))
                    return NotFound();
                else
                    throw;
            }
        }


        [HttpPatch(IDREGEX)]
        public virtual IActionResult Update([FromRoute]string id) {

            string json;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8)) {
                json = reader.ReadToEndAsync().Result;
            }
            var partialEntity = JsonSerializer.Deserialize<dynamic>(json, JsonSerializationOptions);

            var ePk = GetPrimaryKey(partialEntity);
            var iPk = ParseId(id);

            if (!ePk.EqualsAll(iPk))
                return BadRequest($"The path parameter id ({id}) does not match the provided object's id ({ePk.ToTildaDelimited()})");

            try {
                var updated = Repo.Update(partialEntity, iPk);
                return Ok(updated);
            } catch (DbUpdateConcurrencyException) {
                if (!Repo.Exists(iPk))
                    return NotFound();
                else
                    throw;
            }
        }


        [HttpPatch(ASYNC_IDREGEX)]
        public virtual async Task<IActionResult> UpdateAsync([FromRoute]string id) {

            string json;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8)) {
                json = reader.ReadToEndAsync().Result;
            }

            var partialEntity = JsonSerializer.Deserialize<dynamic>(json, JsonSerializationOptions);
            var ePk = GetPrimaryKey(partialEntity);
            var iPk = ParseId(id);

            if (!ePk.EqualsAll(iPk))
                return BadRequest($"The path parameter id ({id}) does not match the provided object's id ({ePk.ToTildaDelimited()})");

            try {
                var updated = await Repo.Update(partialEntity, iPk);
                return Ok(updated);
            } catch (DbUpdateConcurrencyException) {
                if (!Repo.Exists(iPk))
                    return NotFound();
                else
                    throw;
            }
        }

        
        [HttpDelete(IDREGEX)]
        public virtual IActionResult Delete(string id) {
            try {
                Repo.Delete(ParseId(id));
            } catch (MissingEntityException) {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete(ASYNC_IDREGEX)]
        public async virtual Task<IActionResult> DeleteAsync(string id) {
            try {
                await Repo.DeleteAsync(ParseId(id));
            } catch (MissingEntityException) {
                return NotFound();
            }
            return NoContent();
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
        [HttpGet("linq")]
        public virtual IActionResult GetDynamicLinq(
                [FromQuery]string where = null,
                [FromQuery]string orderBy = null,
                [FromQuery]string select = null,
                [FromQuery]int? skip = null,
                [FromQuery]int? take = null,
                [FromQuery]int? totalRecords = null
                ) {

            if (select != null) {
                var pagedResult = Repo.GetFromDynamicLinq(
                    select, where, orderBy, skip, take, totalRecords);
                var json = JsonSerializer.Serialize(pagedResult);
                return new ContentResult { Content = json, ContentType = "application/json" };
            } else {
                var pagedResult = Repo.GetFromDynamicLinq(
                    where, orderBy, skip, take, totalRecords);
                var json = JsonSerializer.Serialize(pagedResult);
                return new ContentResult { Content = json, ContentType = "application/json" };
            }

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
        [HttpGet("linq/async")]
        public virtual async Task<IActionResult> GetDynamicLinqAsync(
                [FromQuery]string where = null,
                [FromQuery]string orderBy = null,
                [FromQuery]string select = null,
                [FromQuery]int? skip = null,
                [FromQuery]int? take = null,
                [FromQuery]int? totalRecords = null
                ) {
            if (select != null) {
                var pagedResult = await Repo.GetFromDynamicLinqAsync(
                    select, where, orderBy, skip, take, totalRecords);
                var json = JsonSerializer.Serialize(pagedResult);
                return new ContentResult { Content = json, ContentType = "application/json" };
            } else {
                var pagedResult = await Repo.GetFromDynamicLinqAsync(
                    where, orderBy, skip, take, totalRecords);
                var json = JsonSerializer.Serialize(pagedResult);
                return new ContentResult { Content = json, ContentType = "application/json" };
            }
        }



        /// <summary>
        /// Get from DevExtreme DataSourceLoader query string
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <returns></returns>
        [HttpGet("devextreme")]
        public virtual IActionResult GetDevExtreme(
                [FromQuery]string select,
                [FromQuery]string sort,
                [FromQuery]string filter,
                [FromQuery]int skip,
                [FromQuery]int take,
                [FromQuery]string totalSummary,
                [FromQuery]string group,
                [FromQuery]string groupSummary
            ) {
            var loadOptions = DataSourceLoadOptionsBuilder.Build(
                select, sort, filter, skip, take, totalSummary,
                group, groupSummary);

            var result = DataSourceLoader.Load(Repo.Query, loadOptions);
            return Ok(result);
        }


        /// <summary>
        /// Asynchronously get from DevExtreme DataSourceLoader query string
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <returns></returns>
        [HttpGet("devextreme/async")]
        public virtual async Task<IActionResult> GetDevExtremeAsync(
                [FromQuery]string select,
                [FromQuery]string sort,
                [FromQuery]string filter,
                [FromQuery]int skip,
                [FromQuery]int take,
                [FromQuery]string totalSummary,
                [FromQuery]string group,
                [FromQuery]string groupSummary
            ) {
            var loadOptions = DataSourceLoadOptionsBuilder.Build(
                select, sort, filter, skip, take, totalSummary,
                group, groupSummary);

            var result = await Task.Run(()=> DataSourceLoader.Load(Repo.Query, loadOptions));
            return Ok(result);
        }




    }
}
