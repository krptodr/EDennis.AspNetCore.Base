﻿using Castle.DynamicProxy;
using EDennis.AspNetCore.Base.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace EDennis.AspNetCore.Base.Web {
    public static partial class IServiceCollectionExtensions {

        public static IServiceCollection AddScopedTraceable<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class, IHasILogger
            where TImplementation : TInterface => services.AddTraceable<TInterface, TImplementation>(ServiceLifetime.Scoped);

        public static IServiceCollection AddSingletonTraceable<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class, IHasILogger
            where TImplementation : TInterface => services.AddTraceable<TInterface, TImplementation>(ServiceLifetime.Singleton);

        public static IServiceCollection AddTransientTraceable<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class, IHasILogger
            where TImplementation : TInterface => services.AddTraceable<TInterface, TImplementation>(ServiceLifetime.Transient);


        public static IServiceCollection AddScopedTraceable<TImplementation>(this IServiceCollection services)
            where TImplementation : class, IHasILogger => services.AddTraceable<TImplementation, TImplementation>(ServiceLifetime.Scoped);

        public static IServiceCollection AddSingletonTraceable<TImplementation>(this IServiceCollection services)
            where TImplementation : class, IHasILogger => services.AddTraceable<TImplementation, TImplementation>(ServiceLifetime.Singleton);

        public static IServiceCollection AddTransientTraceable<TImplementation>(this IServiceCollection services)
            where TImplementation : class, IHasILogger => services.AddTraceable<TImplementation, TImplementation>(ServiceLifetime.Transient);


        public static IServiceCollection AddTraceable<TInterface, TImplementation>(
            this IServiceCollection services, ServiceLifetime serviceLifetime)
            where TInterface : class, IHasILogger
            where TImplementation : TInterface {

            var constructorParameters = typeof(TImplementation)
                .GetConstructors()
                .FirstOrDefault()
                .GetParameters()
                .Select(x => x.ParameterType)
                .ToArray();

            Type loggerType = constructorParameters
                .FirstOrDefault(t => typeof(ILogger).IsAssignableFrom(t));
            Type loggersType = GetIEnumerableType(loggerType);

            object[] args = new object[constructorParameters.Count()];

            if (constructorParameters.Any(t => t == typeof(HttpClient)))
                services.AddHttpClient(); //adds all HttpClient-related services to collection;
            
            services.Add(new ServiceDescriptor(typeof(TInterface), f => {
                var loggers = f.GetRequiredService(loggersType) as IEnumerable<object>;
                var scopeProperties = f.GetRequiredService<ScopeProperties22>();
                ILogger activeLogger = (ILogger)loggers.ElementAt(scopeProperties.LoggerIndex);
                for (int i = 0; i < args.Length; i++) {

                    if (constructorParameters[i] == typeof(HttpClient)) {
                        var httpClientFactory = f.GetRequiredService<IHttpClientFactory>();
                        var httpClient = httpClientFactory.CreateClient(typeof(ApiClient).Name);
                    }
                    args[i] = f.GetRequiredService(constructorParameters[i]);
                }
                var proxy = (TImplementation)new ProxyGenerator()
                    .CreateClassProxy(typeof(TImplementation), args,
                        new TraceInterceptor(activeLogger, scopeProperties));

                return proxy;
            }, serviceLifetime));

            return services;
        }


        public static IServiceCollection AddSecondaryLoggers(this IServiceCollection services,
            params Type[] types) {

            services.TryAddSingleton(f => {
                var loggers = f.GetRequiredService<IEnumerable<ILogger<object>>>();
                return new DefaultLoggerChooser(loggers);
            });
            for (int i = 0; i < types.Length; i++)
                services.AddSingleton(typeof(ILogger<>), types[i]);

            return services;
        }


        public static IServiceCollection AddSecondaryLoggers<TLoggerChooser>(this IServiceCollection services,
            params Type[] types)
            where TLoggerChooser : LoggerChooser{

            services.TryAddSingleton(f => {
                var loggers = f.GetRequiredService<IEnumerable<ILogger<object>>>();
                var chooser = (ILoggerChooser)Activator.CreateInstance(typeof(TLoggerChooser), new object[] { loggers });
                return chooser;
            });
            for (int i = 0; i < types.Length; i++)
                services.AddSingleton(typeof(ILogger<>), types[i]);

            return services;
        }



        private static Type GetIEnumerableType<T>(T type)
            where T : Type {

            var iEnumerableType = typeof(IEnumerable<>);
            var constructedIEnumerableType = iEnumerableType.MakeGenericType(type);

            return constructedIEnumerableType;
        }




    }
}
