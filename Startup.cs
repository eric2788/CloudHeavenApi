using System;
using CloudHeavenApi.Contexts;
using CloudHeavenApi.Features;
using CloudHeavenApi.Implementation;
using CloudHeavenApi.MiddleWaresAndFilters;
using CloudHeavenApi.Models;
using CloudHeavenApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CloudHeavenApi
{
    public class Startup
    {
        internal static readonly string[] AllowOrigins =
        {
            "https://easonchu7.github.io", "https://www.mcchv.space/"
        };

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private void UseMySQL(DbContextOptionsBuilder builder)
        {
            builder.UseMySql(Configuration.GetConnectionString("CloudHeavenDb"), opt => opt.EnableRetryOnFailure());
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<HeavenContext>(UseMySQL);

            services.AddCors(setup =>
            {
                setup.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(AllowOrigins)
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });

                setup.AddPolicy("public", builder => { builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
            });

            services.AddSingleton<IAuthService, MojangService>();
            services.RegisterCache<Identity>();
            services.AddSingleton<WebSocketTable>();
            services.LoadSocketMessageHandler();
            services.AddSingleton<IWebSocketService, HeavenSocketHandler>();
            services.AddTransient<WebSocketMiddleware>();

            services.AddControllersWithViews().AddNewtonsoftJson(opt =>
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            services.AddMvc(setup => setup.Filters.Add<ExceptionFilters>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            var options = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(20),
                ReceiveBufferSize = 4 * 1024
            };
            foreach (var allowOrigin in AllowOrigins) options.AllowedOrigins.Add(allowOrigin);

            app.UseWebSockets();

            app.UseRouting();

            app.UseCors();

            app.Map("/ws", _app => _app.UseMiddleware<WebSocketMiddleware>());

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}