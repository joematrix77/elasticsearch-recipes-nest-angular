﻿namespace ElasticsearchRecipes
{
    using Elastic;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.StaticFilesEx;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // Get the connection settings from appsettings.json and inject them into ElasticConnectionSettings
            services.AddOptions();
            services.Configure<ElasticConnectionSettings>(Configuration.GetSection("ElasticConnectionSettings"));
            
            services.AddSingleton(typeof(ElasticClientProvider));
            services.AddTransient(typeof(DataIndexer));

            services.AddTransient(typeof(SearchService));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                // Always serve the static index.html file, which is the entry point for the Angular app
                routes.MapRoute(
                    name: "default",
                    template: "{*url}",
                    defaults: new
                    {
                        Controller = "Home",
                        Action = "Recipes"
                    });
            });
        }
    }
}
