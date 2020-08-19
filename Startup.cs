using CloudHeavenApi.Contexts;
using CloudHeavenApi.MiddleWaresAndFilters;
using CloudHeavenApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CloudHeavenApi
{
    public class Startup
    {
        internal static readonly string[] AllowOrigins =
        {
            "http://localhost:5000"
        };

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private void UseMySQL(DbContextOptionsBuilder builder)
        {
            builder.UseMySql(Configuration.GetConnectionString("CloudHeavenDb"));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<HeavenContext>(UseMySQL);

            services.AddCors(setup =>
            {
                setup.AddPolicy("private", builder =>
                {
                    builder.WithOrigins(AllowOrigins)
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });

                setup.AddDefaultPolicy(builder => { builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
            });

            services.AddSingleton<IAuthService, MojangService>();

            services.AddControllersWithViews();

            services.AddMvc(setup => setup.Filters.Add<ExceptionFilters>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        }
    }
}