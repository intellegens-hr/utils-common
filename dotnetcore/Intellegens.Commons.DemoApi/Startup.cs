using Intellegens.Commons.DemoApi.DbContext;
using Intellegens.Commons.DemoApi.Dto;
using Intellegens.Commons.DemoApi.Mapper;
using Intellegens.Commons.DemoApi.Services;
using Intellegens.Commons.Search;
using Intellegens.Commons.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Intellegens.Commons.DemoApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DemoDbContext dbContext)
        {
            dbContext.Database.Migrate();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Intellegens.Commons.DemoApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services
                .AddDbContext<DemoDbContext>()
                .AddAutoMapper(typeof(DemoApiMapperProfile))
                .AddSingleton<ISearchServiceFactory, SearchServiceFactory>()
                .AddScoped<IRepositoryBase<PersonDto>, PersonService>()
                .AddScoped<IRepositoryBase<StudentDto>, StudentService>()
                .AddScoped<IRepositoryBase<LecturerDto>, LecturerService>()
                .AddScoped<IRepositoryBase<CourseDto>, CourseService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Intellegens.Commons.DemoApi", Version = "v1" });
            });
        }
    }
}