using EDennis.AspNetCore.Base;
using EDennis.AspNetCore.Base.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace EDennis.Samples.MockHeadersMiddlewareApi.Lib {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();

            //configure ScopePropertiesMiddleware
            var _ = new ServiceConfig(services, Configuration)
                .AddScopedConfiguration()
                .AddMockHeaders();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseScopedConfiguration();

            //for testing purposes:
            //intercept request to add headers 
            app.Use(async (context, next) => {

                //add headers from query string
                foreach (var query in context.Request.Query.Where(q => q.Key.StartsWith("header*")))
                    context.Request.Headers.Add(query.Key.Substring("header*".Length), query.Value);

                await next();

            });

            app.UseMockHeaders();


            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}