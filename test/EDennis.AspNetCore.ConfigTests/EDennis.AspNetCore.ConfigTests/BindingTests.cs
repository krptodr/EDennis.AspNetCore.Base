﻿using EDennis.AspNetCore.Base;
using EDennis.NetCoreTestingUtilities.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.AspNetCore.ConfigTests {
    public class BindingTests {
        private readonly ITestOutputHelper _output;
        public BindingTests(ITestOutputHelper output) {
            _output = output;
        }

        private static readonly ScopePropertiesSettings[] sps =
            new ScopePropertiesSettings[] {
                new ScopePropertiesSettings {
                    UserSource = new HashSet<UserSource> { UserSource.JwtNameClaim },
                    CopyHeaders = true,
                    CopyClaims = true,
                    AppendHostPath = false
                },
                new ScopePropertiesSettings {
                    UserSource = new HashSet<UserSource> { UserSource.XUserHeader },
                    CopyHeaders = false,
                    CopyClaims = false,
                    AppendHostPath = true
                },
                new ScopePropertiesSettings {
                    UserSource = new HashSet<UserSource> { UserSource.JwtNameClaim, UserSource.SessionId },
                    CopyHeaders = true,
                    CopyClaims = true,
                    AppendHostPath = true
                },
                new ScopePropertiesSettings {
                    UserSource = new HashSet<UserSource> { UserSource.JwtSubjectClaim, UserSource.SessionId },
                    CopyHeaders = false,
                    CopyClaims = false,
                    AppendHostPath = false
                },
                new ScopePropertiesSettings {
                    UserSource = new HashSet<UserSource> {
                        UserSource.JwtNameClaim,
                        UserSource.JwtPreferredUserNameClaim,
                        UserSource.JwtSubjectClaim,
                        UserSource.SessionId,
                        UserSource.XUserHeader,
                        UserSource.XUserQueryString,
                        UserSource.OasisNameClaim,
                        UserSource.OasisEmailClaim,
                        UserSource.JwtEmailClaim,
                        UserSource.JwtPhoneClaim,
                        UserSource.JwtClientIdClaim
                    },
                    CopyHeaders = false,
                    CopyClaims = false,
                    AppendHostPath = true
                },

            };

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void ScopeProperties(int testCase) {
            var path = $"ScopeProperties/{testCase}.json";
            var config = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();
            var actual = new ScopePropertiesSettings();
            config.Bind("ScopeProperties",actual);

            var expected = sps[testCase];

            Assert.Equal(expected.UserSource, actual.UserSource);
            Assert.Equal(expected.CopyHeaders, actual.CopyHeaders);
            Assert.Equal(expected.CopyClaims, actual.CopyClaims);
            Assert.Equal(expected.AppendHostPath, actual.AppendHostPath);

        }


        private static readonly MockHeaderSettingsCollection[] mhsc =
            new MockHeaderSettingsCollection[] {
                new MockHeaderSettingsCollection {
                    {"X-Role", new MockHeaderSettings {
                        Values = new string[] { "readonly" },
                        ConflictResolution = MockHeaderConflictResolution.Overwrite }
                    },
                    {"X-User", new MockHeaderSettings {
                        Values = new string[] { "curly@stooges.org" },
                        ConflictResolution = MockHeaderConflictResolution.Overwrite }
                    }
                },
                new MockHeaderSettingsCollection {
                    {"X-Role", new MockHeaderSettings {
                        Values = new string[] { "user" },
                        ConflictResolution = MockHeaderConflictResolution.Union }
                    },
                    {"X-User", new MockHeaderSettings {
                        Values = new string[] { "larry@stooges.org" },
                        ConflictResolution = MockHeaderConflictResolution.Overwrite }
                    }
                },
                new MockHeaderSettingsCollection {
                    {"X-Role", new MockHeaderSettings {
                        Values = new string[] { "admin", "user" },
                        ConflictResolution = MockHeaderConflictResolution.Union }
                    },
                    {"X-User", new MockHeaderSettings {
                        Values = new string[] { "moe@stooges.org" },
                        ConflictResolution = MockHeaderConflictResolution.Overwrite }
                    }
                }
            };



        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void MockHeaders(int testCase) {
            var path = $"MockHeaders/{testCase}.json";
            var config = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();
            var actual = new MockHeaderSettingsCollection();
            config.Bind("MockHeaders",actual);

            var expected = mhsc[testCase];

            Assert.True(actual.IsEqualOrWrite(expected,_output, true));

        }

        private static readonly UserLoggerSettings[] ul =
            new UserLoggerSettings[] {
                new UserLoggerSettings {
                   UserSource = new HashSet<UserSource> { UserSource.JwtNameClaim }
                },
                new UserLoggerSettings {
                   UserSource = new HashSet<UserSource> { UserSource.XUserHeader }
                },
                new UserLoggerSettings {
                   UserSource = new HashSet<UserSource> { UserSource.JwtNameClaim, UserSource.SessionId }
                },
                new UserLoggerSettings {
                   UserSource = new HashSet<UserSource> { UserSource.JwtSubjectClaim, UserSource.SessionId }
                },
                new UserLoggerSettings {
                    UserSource = new HashSet<UserSource> {
                        UserSource.JwtNameClaim,
                        UserSource.JwtPreferredUserNameClaim,
                        UserSource.JwtSubjectClaim,
                        UserSource.SessionId,
                        UserSource.XUserHeader,
                        UserSource.XUserQueryString,
                        UserSource.OasisNameClaim,
                        UserSource.OasisEmailClaim,
                        UserSource.JwtEmailClaim,
                        UserSource.JwtPhoneClaim,
                        UserSource.JwtClientIdClaim
                    }
                },

            };

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void UserLogger(int testCase) {
            var path = $"UserLogger/{testCase}.json";
            var config = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();
            var actual = new UserLoggerSettings();
            config.Bind("UserLogger", actual);

            var expected = ul[testCase];

            Assert.Equal(expected.UserSource, actual.UserSource);

        }



        private static readonly ActiveMockClientSettings[] amcs =
            new ActiveMockClientSettings[] {
                new ActiveMockClientSettings {
                    ActiveMockClientKey = "MockClient1",
                    MockClients = new MockClientSettingsDictionary {
                        {
                            "MockClient1", new MockClientSettings {
                            ClientId = "EDennis.Samples.SomeApi1",
                            ClientSecret = "some secret 1",
                            Scopes = new string[] { "some scope 1" }}
                        },
                        {
                            "MockClient2", new MockClientSettings {
                            ClientId = "EDennis.Samples.SomeApi2",
                            ClientSecret = "some secret 2",
                            Scopes = new string[] { "some scope 2a", "some scope 2b" }}
                        }
                    }
                },
                new ActiveMockClientSettings {
                    ActiveMockClientKey = "MockClient2",
                    MockClients = new MockClientSettingsDictionary {
                        {
                            "MockClient1", new MockClientSettings {
                            ClientId = "EDennis.Samples.SomeApi1",
                            ClientSecret = "some secret 1",
                            Scopes = new string[] { "some scope 1" }}
                        },
                        {
                            "MockClient2", new MockClientSettings {
                            ClientId = "EDennis.Samples.SomeApi2",
                            ClientSecret = "some secret 2",
                            Scopes = new string[] { "some scope 2a", "some scope 2b" }}
                        }
                    }
                }
            };

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void MockClients(int testCase) {
            var path = $"MockClient/{testCase}.json";
            var config = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();
            var actual = new ActiveMockClientSettings();
            config.Bind("MockClient", actual);

            var expected = amcs[testCase];

            Assert.True(actual.IsEqualOrWrite(expected, _output, true));

        }


        private static readonly HeadersToClaims[] htc =
            new HeadersToClaims[] {
                new HeadersToClaims {
                    PreAuthentication = new PreAuthenticationHeadersToClaims{
                        { "X-Role", "role" },
                        { "X-User", "name" }
                    }
                },
                new HeadersToClaims {
                    PreAuthentication = new PreAuthenticationHeadersToClaims{
                        { "X-UserScope", "user_scope" },
                    },
                    PostAuthentication = new PostAuthenticationHeadersToClaims{
                        { "X-Role", "role" },
                        { "X-User", "name" }

                    },
                }
            };

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void HeadersToClaims(int testCase) {
            var path = $"HeadersToClaims/{testCase}.json";
            var config = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();
            var actual = new HeadersToClaims();
            config.Bind("HeadersToClaims", actual);

            var expected = htc[testCase];

            Assert.True(actual.IsEqualOrWrite(expected, _output, true));

        }

        internal class MyDbContext1 : DbContext { }
        internal class MyDbContext2 : DbContext { }
        internal class MyDbContext3 : DbContext { }

        private static readonly dynamic[] dcsd =
            new dynamic[] {
                new DbContextSettings<MyDbContext1> {
                     DatabaseProvider = DatabaseProvider.Sqlite,
                      ConnectionString = "Some connection string 1",
                      Interceptor = new DbContextInterceptorSettings<MyDbContext1> {
                          InstanceNameSource = new HashSet<UserSource> { UserSource.JwtNameClaim },
                          IsInMemory = false,
                          IsolationLevel = IsolationLevel.ReadCommitted,
                          ResetSqlServerIdentities =false,
                          ResetSqlServerSequences = false
                      }
                },
                new DbContextSettings<MyDbContext2> {
                     DatabaseProvider = DatabaseProvider.SqlServer,
                      ConnectionString = "Some connection string 2",
                      Interceptor = new DbContextInterceptorSettings<MyDbContext2> {
                          InstanceNameSource = new HashSet<UserSource> { UserSource.JwtNameClaim },
                          IsInMemory = false,
                          IsolationLevel = IsolationLevel.ReadUncommitted,
                          ResetSqlServerIdentities =false,
                          ResetSqlServerSequences = false
                      }
                },
                new DbContextSettings<MyDbContext3> {
                     DatabaseProvider = DatabaseProvider.InMemory,
                      ConnectionString = "Some connection string 3",
                      Interceptor = new DbContextInterceptorSettings<MyDbContext3> {
                          InstanceNameSource = new HashSet<UserSource> { UserSource.SessionId },
                          IsInMemory = true,
                          IsolationLevel = IsolationLevel.Unspecified,
                          ResetSqlServerIdentities =false,
                          ResetSqlServerSequences = false
                      }
                }

            };

        private static readonly dynamic[] dcsdNew =
            new dynamic[] {
                new DbContextSettings<MyDbContext1>(),
                new DbContextSettings<MyDbContext2>(),
                new DbContextSettings<MyDbContext3>()
            };


        private void TestDbContext<T>(IConfigurationRoot config, int testCase)
            where T : DbContext {
            DbContextSettings<T> actual = dcsdNew[testCase];
            config.Bind($"DbContexts:{typeof(T).Name}", actual);
            DbContextSettings<T> expected = dcsd[testCase];
            Assert.True(actual.IsEqualOrWrite(expected, _output, true));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void DbContexts(int testCase) {
            var path = $"DbContexts/{testCase}.json";
            var config = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            switch (testCase) {
                case 0:
                    TestDbContext<MyDbContext1>(config, testCase);
                    break;
                case 1:
                    TestDbContext<MyDbContext2>(config, testCase);
                    break;
                case 2:
                    TestDbContext<MyDbContext3>(config, testCase);
                    break;
                default:
                    break;
            }

        }


        readonly Apis[] apis = new Apis[] {
            new Apis {
                { "Api1",new Api {
                    Host = "localhost",
                    HttpPort = 6006,
                    HttpsPort = 6005,
                    Mappings = new ApiMappings {
                         ClaimsToHeaders = Api.DEFAULT_CLAIMS_TO_HEADERS,
                         HeadersToHeaders = Api.DEFAULT_HEADERS_TO_HEADERS
                    },
                    ProjectName = "My.Api1",
                    Scheme = "https",
                    Scopes = new string[] { "My.Api1.*" },
                    Version = 1.0M  }
                }
            },
            new Apis {
                { "Api2",new Api {
                    Host = "localhost2",
                    HttpPort = 5678,
                    HttpsPort = 1234,
                    Mappings = new ApiMappings {
                         ClaimsToHeaders = new ClaimsToHeaders {
                             {"name", "X-User2" },
                             { "role", "X-Role2" }
                         },
                         HeadersToHeaders = new HeadersToHeaders {
                             {"X-Testing-Instance", "X-Testing-Instance2" },
                             {"X-Set-ScopedLogger", "X-Set-ScopedLogger2" },
                             {"X-Clear-ScopedLogger", "X-Clear-ScopedLogger2" },
                             {"X-User", "X-User2" },
                             {"X-Role", "X-Role2" },
                             {"X-HostPath", "X-HostPath2" }                         }
                    },
                    ProjectName = "My.Api2",
                    Scheme = "https",
                    Scopes = new string[] { "scope1", "scope2" },
                    Version = 1.1M  }
                },
            },
            new Apis {
                { "IdentityServerApi1",new Api {
                    Host = "localhost",
                    HttpPort = 5001,
                    HttpsPort = 5000,
                    Mappings = null,
                    ProjectName = "IdentityServer",
                    Scheme = "https",
                    Scopes = null,
                    Version = 1.0M,
                    Oidc = new Oidc {
                        Authority = "https://localhost:5000",
                        Audience = "some audience",
                        RequireHttpsMetadata = false,
                        SaveTokens = false,
                        ExclusionPrefix = "-",
                        ClientSecret = "secret",
                        ResponseType = "code id_token",
                        GetClaimsFromUserInfoEndpoint = false,
                        AddOfflineAccess = true,
                        UserScopePrefix = "user_",
                        ClearDefaultInboundClaimTypeMap = true }
                    }
                },
            },
            new Apis {
                { "IdentityServerApi2",new Api {
                    Host = "localhost",
                    HttpPort = 5001,
                    HttpsPort = 5000,
                    Mappings = null,
                    ProjectName = "IdentityServer",
                    Scheme = "https",
                    Scopes = null,
                    Version = 1.0M,
                    OAuth = new OAuth {
                        Authority = "https://localhost:5000",
                        Audience = "some audience",
                        RequireHttpsMetadata = false,
                        SaveTokens = true,
                        ExclusionPrefix = "-",
                        ClientSecret = "secret",
                        ClearDefaultInboundClaimTypeMap = true }
                    }
                },

            }
        };

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Apis(int testCase) {
            var path = $"Apis/{testCase}.json";
            var config = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            var actual = new Apis();
            config.Bind("Apis", actual);
            
            //this is ridiculous -- just require including audience and authority
            //special rebind for subsettings that inherit from parent
            //foreach(var key in actual.Keys) {
            //    var api = actual[key];
            //    if (api.OAuth != null) {
            //        config.GetSection($"Apis:{key}").Bind(api.OAuth);
            //        config.GetSection($"Apis:{key}:OAuth").Bind(api.OAuth);
            //    }
            //    else if (api.Oidc != null) {
            //        config.GetSection($"Apis:{key}").Bind(api.Oidc);
            //        config.GetSection($"Apis:{key}:Oidc").Bind(api.Oidc);
            //    }
            //}

            var expected = apis[testCase];

            Assert.True(actual.IsEqualOrWrite(expected, _output, true));


        }


    }

}