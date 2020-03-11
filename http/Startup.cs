using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using http.Authorization;
using http.Twilio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using postgres;
using postgres.Pgres2;
using PublicCallers.Scheduling;
using scheduling;

namespace http
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Conf = configuration;
        }

        public IConfiguration Conf { get; }

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
                    Conf
                        .GetSection("Bearer")
                        .Bind(opts);
                });

            services.AddAuthorization(options =>
            {
                var issuer = Conf["Bearer:Authority"];
                options.AddPolicy(
                    "bookings",
                    p => p.Requirements.Add(new ScopeRequirement("bookings", issuer)));
                options.AddPolicy(
                    "publish",
                    p => p.Requirements.Add(new ScopeRequirement("publish", issuer))
                );
            });

            if (Conf["Dataprotection:Type"] == "Docker")
            {
                services.AddDataProtection()
                    .PersistKeysToFileSystem(
                        new DirectoryInfo(Conf["Dataprotection:KeyPath"])
                    )
                    .ProtectKeysWithCertificate(
                        new X509Certificate2(
                            Conf["Dataprotection:CertPath"],
                            Conf["Dataprotection:CertPass"]
                        )
                    );
            }

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
            services.AddSingleton<PgresUser>(PgresUserFromConfig(Conf));
            services.AddScoped<IPublisherRepository, PublisherRepo>();
            services.AddSingleton<IDataSource, PgresDb>();
            services.Configure<TwilioOptions>(Conf.GetSection("Twilio"));

            services.AddControllers();
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
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors("PublicData");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(builder =>
            {
                builder.MapControllers();
            });
        }
    }
}
