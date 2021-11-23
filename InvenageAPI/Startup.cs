using InvenageAPI.Services.Extension;
using InvenageAPI.Services.Filter;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Compression;

namespace InvenageAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate()
                .AddCertificateCache();

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();

                options.EnableForHttps = true;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(AuthFilter));
                options.Filters.Add(typeof(NameFilter));
            });

            services.AddSwaggerGen(c =>
            {
                c.GetDefaultSwaggerDoc(Configuration);
            });

            services.AddHttpContextAccessor();

            services.AddDependents();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();

            // Add HSTS Response
            app.UseHsts();

            // Rediect to HTTPS
            app.UseHttpsRedirection();

            // Compress Response
            app.UseResponseCompression();

            app.UseStaticFiles();

            // Add Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Invenage API v1"));

            app.UseExceptionMiddleware();

            // Add Traffic Log
            app.UseLogMiddleware();

            // Add CORS Checking
            app.UseCORSMiddleware();

            // Add Routing
            app.UseRouting();

            // Add Authorization
            app.UseAuthorization();

            // Add Endpoint
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
