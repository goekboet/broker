using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using http.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using postgres;
using PublicCallers.Scheduling;

namespace http
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public IConfiguration Configuration { get; }
        public ILogger<Startup> Logger { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;
                
                // Per default kestrel only forwards proxy-headers from localhost. You need to add
                // ip-numbers into these lists or empty them to forward all.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            
            services.AddAuthentication("Ids")
                .AddJwtBearer("Ids", opts =>
                {
                    Configuration
                        .GetSection("Bearer")
                        .Bind(opts);
                });
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "bookings", 
                    policy => policy.Requirements.Add(new ScopeRequirement("bookings", "https://ids.ego")));
            });

            services.AddCors(opts =>
            {
                opts.AddPolicy("PublicData", b => 
                {
                    b.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .WithMethods(HttpMethod.Get.Method);
                });
            });

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
            services.AddSingleton<PgresUser>(PgresUserFromConfig(Configuration));
            services.AddScoped<IMeetsRepository, PostGresRepo>();

            // services.AddSingleton<PgresUser>
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        private PgresUser PgresUserFromConfig(IConfiguration c) =>
            new PgresUser(
                c["Pgres:Host"],
                c["Pgres:Port"],
                c["Pgres:Handle"],
                c["Pgres:Pwd"],
                c["Pgres:Db"]
            );

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("PublicData");
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
