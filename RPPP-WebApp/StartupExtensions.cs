using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Model;
using RPPP_WebApp.ModelsValidation;
using RPPP_WebApp.Controllers;

namespace RPPP_WebApp
{
    public static class StartupExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration;
            var appSection = configuration.GetSection("AppSettings");
            builder.Services.Configure<AppSettings>(appSection);
            builder.Services.AddDbContext<Rppp04Context>(options => options.UseSqlServer(configuration.GetConnectionString("RPPP04")));
            builder.Services.AddControllersWithViews();
          
            builder.Services.AddTransient<NaruciteljController>();
            builder.Services.AddTransient<VrstaUlogeApiController>();
            builder.Services.AddTransient<VrstaRadaApiController>();

            builder.Services
               .AddControllers()
               .AddJsonOptions(configure => configure.JsonSerializerOptions.PropertyNamingPolicy = null);

            builder.Services
              .AddFluentValidationAutoValidation()
              .AddFluentValidationClientsideAdapters()
              .AddValidatorsFromAssemblyContaining<SuradnikValidator>()
              .AddValidatorsFromAssemblyContaining<SuradnikUlogaValidator>()
              .AddValidatorsFromAssemblyContaining<VrstaUlogeValidator>()
              .AddValidatorsFromAssemblyContaining<VrstaUlogeViewModelValidator>()
              .AddValidatorsFromAssemblyContaining<ZadatakValidator>()
              .AddValidatorsFromAssemblyContaining<EvidencijaValidator>()
              .AddValidatorsFromAssemblyContaining<StatusValidator>()
              .AddValidatorsFromAssemblyContaining<VrstaRadaValidator>()
              .AddValidatorsFromAssemblyContaining<KorisnickiRacunValidator>();
            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            #region Needed for nginx and Kestrel (do not remove or change this region)
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                 ForwardedHeaders.XForwardedProto
            });
            string pathBase = app.Configuration["PathBase"];
            if (!string.IsNullOrWhiteSpace(pathBase))
            {
                app.UsePathBase(pathBase);
            }
            #endregion

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseStaticFiles()
               .UseRouting()
               .UseEndpoints(endpoints =>
               {
                   endpoints.MapDefaultControllerRoute();
               });

            return app;
        }
    }
}
