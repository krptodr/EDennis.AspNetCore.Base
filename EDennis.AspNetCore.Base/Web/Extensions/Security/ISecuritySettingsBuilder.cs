﻿using EDennis.AspNetCore.Base.Logging;
using EDennis.AspNetCore.Base.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


namespace EDennis.AspNetCore.Base.Web {

    public interface ISecuritySettingsBuilder { 
        SecuritySettings SecuritySettings { get; set; }
        IServiceCollection ServiceCollection { get; set; } 
        IConfiguration Configuration { get; set; }
        string ConfigurationKey { get; set; }
    }

    public class SecuritySettingsBuilder : ISecuritySettingsBuilder {
        public SecuritySettings SecuritySettings { get; set; }
        public IServiceCollection ServiceCollection { get; set; }
        public IConfiguration Configuration { get; set; }
        public string ConfigurationKey { get; set; }
    }



    public static class ISecuritySettingsBuilder_Extensions {

        public static ISecuritySettingsBuilder AddAuthorization<TStartup>(this IApiSettingsBuilder builder) {
            builder.ServiceCollection.Configure<ApiLauncherSettings>(options => {
                options.Host = builder.ApiSettings.Host;
                options.Version = builder.ApiSettings.Version;
                options.Scheme = builder.ApiSettings.Scheme;
                options.HttpsPort = builder.ApiSettings.HttpsPort;
                options.HttpPort = builder.ApiSettings.HttpPort;
                options.NeedsLaunched = builder.ApiSettings.Launcher.NeedsLaunched;
                options.ProjectName = builder.ApiSettings.Launcher.ProjectName;
            });

            builder.ServiceCollection.PostConfigure<ApiLauncherSettings>(options => {
                options.Facade = new ApiSettingsFacade {
                    Configuration = builder.Configuration,
                    ConfigurationKey = builder.ConfigurationKey
                };
            });
            return builder;
        }


        public static IApiSettingsBuilder AddClient<TClientImplementation>(this IApiSettingsBuilder builder, bool traceable = true)
            where TClientImplementation : ApiClient {
            builder.AddDependencies<TClientImplementation>();
            builder.AddApiClientInternal<TClientImplementation, TClientImplementation>(traceable);
            return builder;
        }

        public static IApiSettingsBuilder AddClient<TClientInterface, TClientImplementation>(this IApiSettingsBuilder builder, bool traceable = true)
            where TClientInterface : class, IHasILogger
            where TClientImplementation : ApiClient, TClientInterface {

            builder.AddDependencies<TClientImplementation>();
            builder.AddApiClientInternal<TClientInterface, TClientImplementation>(traceable);
            return builder;
        }

        private static void AddDependencies<TClientImplementation>(this IApiSettingsBuilder builder)
            where TClientImplementation : class {

            builder.ServiceCollection.TryAddScoped<IScopeProperties, ScopeProperties>();

            bool isSecureClient = typeof(SecureApiClient).IsAssignableFrom(typeof(TClientImplementation));

            if (isSecureClient) {
                builder.ServiceCollection.TryAddSingleton<SecureTokenService, SecureTokenService>();
                builder.ServiceCollection.TryAddScoped<IdentityServerApi>();
            }
        }

        private static IApiSettingsBuilder AddApiClientInternal<TClientInterface, TClientImplementation>(this IApiSettingsBuilder builder, bool traceable)
            where TClientInterface : class, IHasILogger
            where TClientImplementation : ApiClient, TClientInterface {


            builder.ServiceCollection.Configure<ApiClientSettings>(options => {
                options.Host = builder.ApiSettings.Host;
                options.Version = builder.ApiSettings.Version;
                options.Scheme = builder.ApiSettings.Scheme;
                options.HttpsPort = builder.ApiSettings.HttpsPort;
                options.HttpPort = builder.ApiSettings.HttpPort;
                options.Mappings = builder.ApiSettings.Client.Mappings;
                options.Scopes = builder.ApiSettings.Client.Scopes;
            });


            if (traceable)
                builder.ServiceCollection.AddScopedTraceable<TClientInterface, TClientImplementation>();
            else
                builder.ServiceCollection.AddHttpClient<TClientInterface, TClientImplementation>();

            return builder;
        }



    }
}
