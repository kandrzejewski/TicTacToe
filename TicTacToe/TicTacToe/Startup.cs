using System;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using TicTacToe.Services;
using TicTacToe.Extensions;
using Microsoft.Extensions.Configuration;
using TicTacToe.Filters;
using TicTacToe.ViewEngines;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TicTacToe.Data;
using Microsoft.EntityFrameworkCore;

namespace TicTacToe
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public IConfiguration _configuration { get; }
        public IHostEnvironment _hostingEnvironment { get; }

        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostEnvironment;
        }

        public void ConfigureCommonServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Localization");
            services.AddMvc(o => o.Filters.Add(typeof(DetectMobileFilter)))
                .AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix,
                    options => options.ResourcesPath = "Localization")
                .AddDataAnnotationsLocalization();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IGameInvitationService, GameInvitationService>();
            services.Configure<Options.EmailServiceOptions>(_configuration.GetSection("Email"));
            services.AddSingleton<IGameSessionService, GameSessionService>();
            services.AddEmailService(_hostingEnvironment, _configuration);
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IEmailViewEngine, EmailViewEngine>();
            services.AddTransient<IEmailTemplateRenderService, EmailTemplateRenderService>();
            services.AddRouting();
            services.AddSession(o => o.IdleTimeout = TimeSpan.FromMinutes(30));

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<GameDbContext>((serviceProvider, options) =>
                options.UseSqlServer(connectionString)
                .UseInternalServiceProvider(serviceProvider));
            var dbContextOptionsbuilder = new DbContextOptionsBuilder<GameDbContext>()
                .UseSqlServer(connectionString);
            services.AddSingleton(dbContextOptionsbuilder.Options);
        }
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
        }

        public void ConfigureStagingServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
        }

        //public void ConfigureServices(IServiceCollection services)
        //{         
        //    services.AddDirectoryBrowser();
        //    services.AddSession(o => o.IdleTimeout = TimeSpan.FromMinutes(30));
        //    services.AddSingleton<IUserService, UserService>();
        //    services.Configure<Options.EmailServiceOptions>(_configuration.GetSection("Email"));
        //    services.AddSingleton<IEmailService, EmailService>();
        //    services.AddSingleton<IGameInvitationService, GameInvitationService>();
        //    services.AddLocalization(options => options.ResourcesPath = "Localization");
        //    services.AddMvc().AddViewLocalization(
        //        LanguageViewLocationExpanderFormat.Suffix,
        //        options => options.ResourcesPath = "Localization")
        //        .AddDataAnnotationsLocalization();
        //}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSession();  //U�ywanie sesji

            //Przegl�danie plik�w
            //app.UseDirectoryBrowser();
 
            app.UseRouting();

            app.UseWebSockets();

            app.UseCommunicationMiddleware();

            //Oprogramiowanie lokalizacji
            var supportedCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("pl-PL"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };
            localizationOptions.RequestCultureProviders.Clear();
            localizationOptions.RequestCultureProviders.Add(new CultureProviderResolverService());
            app.UseRequestLocalization(localizationOptions);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "MyArea",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseStatusCodePagesWithRedirects("/error/{0}");

            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<GameDbContext>().Database.Migrate();
            }
        }
    }
}
