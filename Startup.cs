using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SurveyUP.Data;
using SurveyUP.Data.Enums;
using SurveyUP.Models;
using SurveyUP.Models.Tables;
using SurveyUP.Services;

using System;

namespace SurveyUP
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
            // SQL Server is the default. Set "Database:Provider": "Sqlite" for local development without a server.
            var useSqlite = string.Equals(Configuration["Database:Provider"], "Sqlite", StringComparison.OrdinalIgnoreCase);
            void ConfigureDb(DbContextOptionsBuilder options)
            {
                var connectionString = Configuration.GetConnectionString("DefaultConnection");
                if (useSqlite)
                    options.UseSqlite(connectionString).AddInterceptors(new SqliteCompatibilityInterceptor());
                else
                    options.UseSqlServer(connectionString);
            }

            services.AddDbContext<N3mikosContext>(ConfigureDb)
                    .AddDbContext<SurveyUpIdDbContext2>(ConfigureDb);



            services.AddDatabaseDeveloperPageExceptionFilter();

            IdentityBuilder identityBuilder = services.AddIdentity<ApplicationUser, ApplicationRole>
                (options => 
                        options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<SurveyUpIdDbContext2>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            services.AddTransient<IEmailSender, EmailSender>(i =>
                new EmailSender(
                    Configuration["EmailSender:Host"],
                    Configuration.GetValue<int>("EmailSender:Port"),
                    Configuration.GetValue<bool>("EmailSender:EnableSSL"),
                    Configuration["EmailSender:UserName"],
                    Configuration["EmailSender:Password"]
                )
            );

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.MaxAge = TimeSpan.FromHours(3);
            });

            // And the configure methods
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.MaxAge = TimeSpan.FromHours(3);
                options.Cookie.Name = "SurveyUp";
                options.Cookie.HttpOnly = true;

                options.LoginPath = new PathString("/Identity/Account/Login");
                options.AccessDeniedPath = new PathString("/Identity/Account/AccessDenied");
                options.SlidingExpiration = true;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy($"Require{Roles.Administrator}Role",
                        policy => policy.RequireRole(Roles.Administrator.ToString()));
                options.AddPolicy($"Require{Roles.Student}Role",
                    policy => policy.RequireRole(
                        Roles.Administrator.ToString(),
                        Roles.Twórca.ToString(),
                        Roles.Student.ToString()));
                options.AddPolicy($"Require{Roles.Twórca}Role",
                    policy => policy.RequireRole(
                        Roles.Administrator.ToString(),
                        Roles.Twórca.ToString()));
                options.AddPolicy($"Require{Roles.Sekretariat}Role",
                    policy => policy.RequireRole(
                        Roles.Administrator.ToString(),
                        Roles.Twórca.ToString(),
                        Roles.Sekretariat.ToString()));

            });


            services.AddRazorPages(options =>
                {
                    options.Conventions.AuthorizeFolder("/Answers", $"Require{Roles.Twórca}Role");
                    options.Conventions.AuthorizeFolder("/Questions", $"Require{Roles.Twórca}Role");
                    options.Conventions.AuthorizeFolder("/Surveys", $"Require{Roles.Twórca}Role");
                    options.Conventions.AuthorizeFolder("/Users", $"Require{Roles.Administrator}Role");
                    options.Conventions.AuthorizeFolder("/Roles", $"Require{Roles.Administrator}Role");
                    options.Conventions.AuthorizeFolder("/Voters", $"Require{Roles.Sekretariat}Role");
                    options.Conventions.AuthorizeFolder("/Public", $"Require{Roles.Student}Role");
                    
                })
                .AddSessionStateTempDataProvider();



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();


            app.UseCookiePolicy(); // <- this is for tempdata

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
