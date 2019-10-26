﻿using EDennis.AspNetCore.Base.Web;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;

namespace EDennis.AspNetCore.Base.Testing {

    /// <summary>
    /// Creates a mock client with specified client id,
    /// secret, and scope.  A mock client is only created
    /// when there is no Authorization header already.
    /// 
    /// The class uses a client 
    /// configuration defined in appsettings.
    /// 
    /// in appsettings.Development.json:
    /// {
    ///   "IdentityServer": {
    ///     "Authority": "http://localhost:5000",
    ///     "ApiName": "api1",
    ///     "Client": {
    ///       "Authority": "http://localhost:5000",
    ///       "ClientId": "client",
    ///       "Secret": "secret",
    ///       "Scope": "api1"
    ///     }
    ///   }
    /// }
    /// </summary>
    public class MockClientAuthorizationMiddleware22 {


        public const string LAUNCHSETTINGS_PROFILE_KEY = "LaunchSettings:Profile";

        private readonly RequestDelegate _next;

        public MockClientAuthorizationMiddleware22(RequestDelegate next) {
            _next = next;
        }

        /// <summary>
        /// Invoke the middleware
        /// </summary>
        /// <param name="context">The HttpContext</param>
        /// <param name="config">The Configuration</param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, IConfiguration config, 
            IScopeProperties scopeProperties) {

            ConfigurationBinder.

            if (!context.Request.Path.StartsWithSegments(new PathString("/swagger"))) {
                //get a reference to the request headers
                var headers = context.Request.Headers;

                //only create mock client, if no authorization header
                //already exists
                if (!headers.ContainsKey("Authorization")) {

                    //instantiate a new HttpClient for communicating
                    //with Identity Server
                    var client = new HttpClient();

                    //get parameters for building the token request
                    var tokenRequestData = GetClientCredentialsTokenRequestData(config, scopeProperties);

                    //get the discovery document from Identity Server
                    var disco = await client.GetDiscoveryDocumentAsync(tokenRequestData.Authority as string);
                    if (disco.IsError) {
                        throw new SecurityException(disco.Error);
                    }

                    //send a token request and receive back the response
                    var tokenResponse = await client.RequestClientCredentialsTokenAsync(
                        new ClientCredentialsTokenRequest {
                            Address = disco.TokenEndpoint,
                            ClientId = tokenRequestData.ClientId,
                            ClientSecret = tokenRequestData.ClientSecret,
                            Scope = string.Join(' ', tokenRequestData.Scopes),                             
                        });

                    //handle errors
                    if (tokenResponse.IsError) {
                        context.Response.StatusCode = 400;
                        string msg;
                        if (tokenResponse.Error == "invalid_client")
                            msg = $"Client with Id = {tokenRequestData.ClientId} && Secret = {tokenRequestData.ClientSecret} is invalid.";
                        else if (tokenResponse.Error == "invalid_scope")
                            msg = $"One or more of the following scopes are not valid: {string.Join(' ', tokenRequestData.Scopes)}"; 
                        else
                            msg = tokenResponse.Error;
                        await context.Response.WriteAsync(msg);
                        return;
                        //throw new SecurityException(tokenResponse.Error);
                    }

                    //ValidateJwt(disco, tokenResponse);

                    //build the authorization header with bearer token
                    //and add it to the current request headers
                    headers.Add("Authorization", "Bearer " + tokenResponse.AccessToken);

                }
            }


            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }


        private dynamic GetClientCredentialsTokenRequestData(IConfiguration config,
                IScopeProperties scopeProperties ) {
          
            dynamic tokenRequestData;

            try {
                var mockClientDictionary = new Dictionary<string,MockClient>();
                config.GetSection("MockClients").Bind(mockClientDictionary);
                //as a backup look for MockClient
                if (mockClientDictionary == null || mockClientDictionary.Count() == 0)
                    config.GetSection("MockClient").Bind(mockClientDictionary);
            } catch {
                throw new ApplicationException($"Configuration does not contain a valid MockClients section.  MockClients must be a top-level key, followed by named (string) MockClients, each with a valid structure (see MockClientDictionary")
            }

            var mockClientProperties = new MockClientProperties22();

            if (scopeProperties.ActiveProfileName != "Default" || mockClientArg == null) 
                mockClientArg = scopeProperties.Profiles[scopeProperties.ActiveProfileName].MockClientId;                


                //Get either MockClients or MockClient section
                if (mockClientProperties == null)
                    config.GetSection($"MockClient:{mockClientArg}").Bind(mockClientProperties);
                if (mockClientProperties == null)
                    throw new ArgumentException($"MockClientAuthorizationMiddleware requires 'MockClient:{mockClientArg}' configuration key, which is missing.");
                else {
                    tokenRequestData = GetClientCredentialsTokenRequestData(mockClientArg, mockClientProperties);
                }
            } else {
                var defaultMockClient = mockClientDictionary
                    .OrderByDescending(x => x.Value.Default)
                    .ThenBy(x => x.Key)
                    .FirstOrDefault();
                if (defaultMockClient.Key == null)
                    throw new ArgumentException("MockClientAuthorizationMiddleware requires 'MockClient...' configuration key, which is missing.");
                else {
                    tokenRequestData = GetClientCredentialsTokenRequestData(defaultMockClient.Key, defaultMockClient.Value);
                }


            }
            if (tokenRequestData.Authority == null) {

                var apiDict = new Dictionary<string, ApiConfig>();
                config.GetSection("Apis").Bind(apiDict);

                var env = config["ASPNETCORE_ENVIRONMENT"];


                var identityServerApiName = GetIdentityServerApiType().Name;

                if (apiDict.ContainsKey("identityServerApiName")) {
                    throw new ApplicationException($"MockClientAuthorizationMiddleware requires the presence of a Apis config entry that is an identity server. No Api having with Secret = null appears in appsettings.{env}.json.");
                }
                var identityServerApi = apiDict[identityServerApiName];
                if (identityServerApi == null)
                    throw new ApplicationException($"MockClientAuthorizationMiddleware requires the presence of a Apis config entry that is an identity server. No Api having with Secret = null appears in appsettings.{env}.json.");

                authority = identityServerApi.BaseAddress;
            } else {
                authority = tokenRequestData.Authority;
            }

            return new {
                Authority = authority,
                tokenRequestData.ClientId,
                tokenRequestData.ClientSecret,
                tokenRequestData.Scopes
            };

        }



        private dynamic GetClientCredentialsTokenRequestData(string mockClientId,
            MockClientProperties mockClientProperties) {

            string authority = null;
            string clientId;
            string clientSecret;
            string[] scopes;

            clientId = mockClientId;

            if (mockClientProperties.Secret != null)
                clientSecret = mockClientProperties.Secret;
            else
                throw new ArgumentException($"MockClientAuthorizationMiddleware requires 'MockClient:{mockClientId}:Secret' configuration key, which is missing.");

            if (mockClientProperties.Scopes != null && mockClientProperties.Scopes.Count() > 0)
                scopes = mockClientProperties.Scopes;
            else if (mockClientProperties.Scope != null) {
                scopes = mockClientProperties.Scope.Split(' ');
            } else
                throw new ArgumentException($"MockClientAuthorizationMiddleware requires 'MockClient:{mockClientId}:Scopes' configuration key, which is missing.");


            return new {
                Authority = authority,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scopes = scopes
            };

        }

        private static Type GetIdentityServerApiType() {
            var serviceType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(IdentityServerApi)))
                .FirstOrDefault();
            if (serviceType != null)
                return serviceType;
            else
                return typeof(IdentityServerApi);
        }



    }

    /// <summary>
    /// Extension methods for MockClientAuthorizationMiddleware
    /// </summary>
    public static class IApplicationBuilder_MockClientAuthorizationExtensionMethods {

        /// <summary>
        /// Creates a Use... method for the middleware
        /// </summary>
        /// <param name="app">application builder used by Startup.cs</param>
        /// <returns>application builder (for method chaining)</returns>
        public static IApplicationBuilder UseMockClientAuthorization(
                this IApplicationBuilder app) {
            return app.UseMiddleware<MockClientAuthorizationMiddleware>();
        }
    }
}
