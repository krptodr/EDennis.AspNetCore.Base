﻿using EDennis.AspNetCore.Base.EntityFramework;
using EDennis.AspNetCore.Base.Web;
using EDennis.Samples.InternalApi1.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Linq;

namespace EDennis.Samples.InternalApi1 {
    public class Startup {

        public Startup(IConfiguration configuration, IHostingEnvironment env) {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc(options=> {
                if (HostingEnvironment.EnvironmentName == EnvironmentName.Development) {
                    options.Filters.Add<TestingActionFilter>();
                }                
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContextPools(Configuration);
            services.AddRepos();

            //services.AddDbContextPool<HrContext>(options => {
            //    options.UseSqlServer(Configuration.GetConnectionString("HrContext"));
            //});
            //services.AddTransient<EmployeeRepo, EmployeeRepo>();
            //services.AddTransient<PositionRepo, PositionRepo>();
            //services.AddTransient<EmployeePositionRepo, EmployeePositionRepo>();
            //services.AddTransient<ManagerPositionRepo, ManagerPositionRepo>();

            if (HostingEnvironment.EnvironmentName == EnvironmentName.Development) {
                services.AddSingleton<IDbContextBaseTestCache, DbContextBaseTestCache>();
                var provider = services.BuildServiceProvider();
                var cache = provider.GetService<IDbContextBaseTestCache>() as DbContextBaseTestCache;
                var cacheHashCode = cache.GetHashCode();
                var configHashCode = Configuration.GetHashCode();
                var configPaths = Configuration.PrintConfigFilePaths();
                Debug.WriteLine($"InternalApi1.Startup.ConfigureServices with cache ({cacheHashCode}) and config ({configHashCode}: {configPaths[0]}) ");
            }


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}