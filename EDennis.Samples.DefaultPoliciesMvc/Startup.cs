using EDennis.AspNetCore.Base;
using EDennis.AspNetCore.Base.Security;
using EDennis.AspNetCore.Base.Web;
using EDennis.AspNetCore.Base.Web.Extensions;
using EDennis.Samples.DefaultPoliciesMvc.ApiClients;
using EDennis.Samples.DefaultPoliciesMvc.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EDennis.Samples.DefaultPoliciesMvc {
    public class Startup {
        public Startup(IConfiguration configuration, IWebHostEnvironment env) {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllersWithViews();

            services.Configure<CookiePolicyOptions>(options => {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var securityOptions = new SecurityOptions();
            Configuration.GetSection("Security").Bind(securityOptions);

            services.AddClientAuthenticationAndAuthorizationWithDefaultPolicies(securityOptions);

            services.AddControllersWithViews(options => {
                options.Conventions.Add(new AddDefaultAuthorizationPolicyConvention(HostingEnvironment, Configuration));
            });


            services.AddScoped<ScopeProperties>();
            services.AddApiClients<IdentityServerApi>();
            services.AddApiClient<IDefaultPoliciesApiClient, DefaultPoliciesApiClient>();


            services.AddDbContext<AppDbContext>(options =>
                            options.UseSqlite("Data Source=hr.db")
                            );



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app) {
            if (HostingEnvironment.EnvironmentName == "Development") {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();


            //app.UseAuthorization();
            app.UseAuthentication();
            app.UseUser();


            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}