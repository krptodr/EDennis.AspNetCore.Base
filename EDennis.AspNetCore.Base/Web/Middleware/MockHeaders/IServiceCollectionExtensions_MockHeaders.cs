﻿using EDennis.AspNetCore.Base.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.AspNetCore.Base.Web.Middleware.ApiMiddleware {
    public static partial class IServiceCollectionExtensions {
        public static IServiceCollection AddMockHeaders(IServiceCollection services, IConfiguration configuration, string configurationKey) {

            services.Configure<MockHeaderSettingsCollection>(configuration.GetSection(configurationKey));
            return services;
        }
    }
}
