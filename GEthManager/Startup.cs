using GEthManager.Processing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using AsmodatStandard.Extensions;
using GEthManager.Services;

namespace GEthManager
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {

            var configurationFilePath1 = @"D:\GLOBAL\GEthManagerConfig.json";
            string configJson = null;

            if(File.Exists(configurationFilePath1))
            {
                configJson = configurationFilePath1;
            }

            if(configJson.IsNullOrEmpty())
            {
                Configuration = configuration;
                return;
            }
            
            var builder = new ConfigurationBuilder()
                .AddJsonFile(configJson, optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                    });
            });

            services.AddMvc();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); //required for client IP discovery

            services.AddOptions();

            services.Configure<ManagerConfig>(Configuration.GetSection("ManagerConfig"));

            services.AddSingleton<BlockSyncManager>();

            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, EtherScanService>();
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, InfuraScanService>();
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, PublicScanService>();
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, PrivateScanService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseCors("AllowAll");

            app.UseMvc(routes => {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
