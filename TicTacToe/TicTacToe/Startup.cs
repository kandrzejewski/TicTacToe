using System;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using TicTacToe.Services;
using TicTacToe.Extensions;
using TicTacToe.Filters;
using TicTacToe.ViewEngines;
using TicTacToe.Data;
using TicTacToe.Managers;
using TicTacToe.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

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
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, options => options.ResourcesPath = "Localization")
                .AddDataAnnotationsLocalization();

            services.AddTransient<ApplicationUserManager>();
            services.AddTransient<IUserService, UserService>();
            services.AddScoped<IGameInvitationService, GameInvitationService>();
            services.AddScoped<IGameSessionService, GameSessionService>();

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<GameDbContext>((serviceProvider, options) =>
                options.UseSqlServer(connectionString)
                .UseInternalServiceProvider(serviceProvider));
            //var dbContextOptionsbuilder = new DbContextOptionsBuilder<GameDbContext>()
            //    .UseSqlServer(connectionString);
            //services.AddSingleton(dbContextOptionsbuilder.Options);
            services.AddScoped(typeof(DbContextOptions<GameDbContext>), (serviceProvider) =>
            {
                return new DbContextOptionsBuilder<GameDbContext>().UseSqlServer(connectionString).Options;
            });

            services.Configure<Options.EmailServiceOptions>(_configuration.GetSection("Email"));
            services.AddEmailService(_hostingEnvironment, _configuration);
            services.AddTransient<IEmailTemplateRenderService, EmailTemplateRenderService>();
            services.AddTransient<IEmailViewEngine, EmailViewEngine>();

            services.AddRouting();
            services.AddSession(o => o.IdleTimeout = TimeSpan.FromMinutes(30));

            services.AddIdentity<UserModel, RoleModel>(options =>
            {
                options.Password.RequiredLength = 1;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = false;
            }).AddEntityFrameworkStores<GameDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie().AddFacebook(facebook =>
            {
                facebook.AppId = "993484707755228";
                facebook.AppSecret = "b262580798efb6474971027ece3134b2";
                //facebook.ClientId = "123";
                //facebook.ClientSecret = "123";
            });

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>(); 
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

            app.UseAuthentication(); //U¿ywanie uwierzytelniania

            app.UseSession();  //U¿ywanie sesji

            //Przegl¹danie plików
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

            app.UseStatusCodePages("text/plain", "Blad HTTP - kod odpowiedzi: {0}");

            //using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            //{
            //    scope.ServiceProvider.GetRequiredService<GameDbContext>().Database.Migrate();
            //}
            var provider = app.ApplicationServices;
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<GameDbContext>())
            {
                context.Database.Migrate();
            }
        }
    }
}
