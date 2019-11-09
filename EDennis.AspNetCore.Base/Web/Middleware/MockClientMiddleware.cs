﻿using EDennis.AspNetCore.Base.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace EDennis.AspNetCore.Base.Web {

    /// <summary>
    /// *** ENVIRONMENT GUARD - NEVER IN PRODUCTION
    ///
    /// NOTE: Place in pipeline just before Authentication middleware
    /// NOTE: If using MockClient, ensure that the MockClient has been configured
    ///       in IdentityServer, also.  The IdentityServer configuration for the 
    ///       MockClient must include all scopes configured in appsettings.
    /// </summary>
    public class MockClientMiddleware {

        private readonly RequestDelegate _next;
        private readonly IOptionsMonitor<AppSettings> _appSettings;
        private readonly SecureTokenService _tokenService;

        public MockClientMiddleware(RequestDelegate next, 
            IOptionsMonitor<AppSettings> appSettings,
            SecureTokenService tokenService) {
            _next = next;
            _appSettings = appSettings;
            _tokenService = tokenService;
        }


        public async Task InvokeAsync(HttpContext context) {

            //ignore, if swagger meta-data processing
            if (!context.Request.Path.StartsWithSegments(new PathString("/swagger"))) {

                var mockClient = _appSettings.CurrentValue.ActiveMockClient;
                if (mockClient != null) {

                    var tokenResponse = await _tokenService.GetTokenResponse(
                        mockClient.ClientId, mockClient.ClientSecret,
                        mockClient.Scopes);

                    context.Request.Headers.Add("Authorization", "Bearer " + tokenResponse.AccessToken);
                }
            }

            await _next(context);

        }

    }

    public static partial class IApplicationBuilderExtensions_Middleware {
        public static IApplicationBuilder UseMockClient(this IApplicationBuilder app) {
            app.UseMiddleware<MockClientMiddleware>();
            return app;
        }
    }


}
