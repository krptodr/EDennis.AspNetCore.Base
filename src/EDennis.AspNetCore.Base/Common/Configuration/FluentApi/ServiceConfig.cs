﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;

namespace EDennis.AspNetCore.Base {

    /// <summary>
    /// Provides a convenience class for navigating the configuration tree
    /// while holding onto the service collection.  With extension methods
    /// (typically on IServiceConfig), this class can faciliate building a
    /// fluent API for configuration of an application.
    /// </summary>
    public class ServiceConfig : IServiceConfig {

        public IServiceCollection Services { get; }
        public IConfiguration Configuration { get; }
        public MvcOptionsBuilder MvcOptions { get; }

        public ServiceConfig() {
            MvcOptions =  new MvcOptionsBuilder(this);
        }

        public T Bind<T>(string path) where T : class, new() {
            var configSection = Configuration.GetSection(path);
            var obj = new T();
            configSection.Bind(obj);
            if(!_objects.ContainsKey(path))
                _objects.Add(path, obj);
            return obj;
        }

        public void Configure<T>(string path) where T : class, new() {
            var configSection = Configuration.GetSection(path);
            Services.Configure<T>(configSection);
        }

        public T BindAndConfigure<T>(string path) where T : class, new() {
            var configSection = Configuration.GetSection(path);
            Services.Configure<T>(configSection);
            var obj = new T();
            configSection.Bind(obj);
            _objects.Add(path, obj);
            return obj;
        }

        //TODO: Not Needed?
        public T GetObject<T>(string path) where T: class, new() {
            if (!_objects.ContainsKey(path))
                return null;
            return (T)_objects[path];
        }

        //TODO: Not Needed?
        public T GetParentObject<T>(string path) where T: class, new() {
            var parentPath = GetParentPath(path);
            if (!_objects.ContainsKey(parentPath))
                return null;
            return (T)_objects[parentPath];
        }

        public string GetParentPath(string path) {
            string parentPath = "";
            if (path.Contains(":"))
                parentPath = path.Substring(0, path.LastIndexOf(':'));
            return parentPath;
        }


        //TODO: Not Needed?
        readonly Dictionary<string, object> _objects = new Dictionary<string, object>();

        public ServiceConfig(IServiceCollection services, IConfiguration config) {
            Configuration = config;
            Services = services;
        }

    }
}
