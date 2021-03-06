﻿using EDennis.AspNetCore.Base.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace EDennis.AspNetCore.Base.Testing {
    public class ApiClientInterceptor<TClient> : Interceptor
        where TClient : ApiClient {


        public ApiClientInterceptor(RequestDelegate next) : base(next) { }

        public async Task InvokeAsync(HttpContext context, IServiceProvider provider) {

            if (!context.Request.Path.StartsWithSegments(new PathString("/swagger"))) {


                var header = GetTestingHeader(context);

                if (header.Key == null) {

                    var defaultInstanceName = DEFAULT_NAMED_INSTANCE;
                    try {
                        if (context.Session != null)
                            defaultInstanceName = context.Session.Id;
                    } catch { }


                    context.Request.Headers.Add(HDR_USE_INMEMORY, defaultInstanceName);
                    header = new KeyValuePair<string, string>(HDR_USE_INMEMORY, defaultInstanceName);
                }
                string operation = header.Key;
                string instanceName = header.Value;

                var scopeProperties = provider.GetRequiredService(typeof(ScopeProperties)) as ScopeProperties;

                var apiClientHeaders = context.Request.Headers.Where(x => x.Key.StartsWith("X-")).ToList();
                var dict = new Dictionary<string, object> {
                    { ApiClient.HEADER_KEY, apiClientHeaders }
                };

                scopeProperties.OtherProperties.Add(typeof(TClient).Name, dict);

                //if(scopeProperties.OtherProperties.Where(x=>x.Key.StartsWith(Interceptor.HDR_PREFIX)).Count()==0)                    
                //    scopeProperties.OtherProperties.Add(operation, instanceName);

                //var client = provider.GetRequiredService(typeof(TClient)) as TClient;

                if (operation == HDR_DROP_INMEMORY ) {
                    //client.HttpClient.SendResetAsync(operation,instanceName);
                    return;
                }

            }

            await _next(context);

        }

    }


    public static class IApplicationBuilderExtensions_ApiClientInterceptorMiddleware {
        public static IApplicationBuilder UseApiClientInterceptor<TClient>(this IApplicationBuilder app)
        where TClient : ApiClient {
            app.UseMiddleware<ApiClientInterceptor<TClient>>();
            return app;
        }
    }


}
