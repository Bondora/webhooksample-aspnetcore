using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using webhooksite.Config;
using webhooksite.RequestValidator;

namespace webhooksite
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<DataConfig>(Configuration.GetSection("Data"));
            services.Configure<SignatureConfig>(Configuration.GetSection("Signature"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<SignatureConfig> sigOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseSignatureValidation(o =>
            {
                foreach (var kvp in sigOptions.Value.Keys)
                {
                    o.Keys[kvp.Key] = Convert.FromBase64String(kvp.Value);
                }
            });

            app.UseMvc();
        }
    }
}
