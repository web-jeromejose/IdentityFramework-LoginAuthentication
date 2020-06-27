using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quiz_Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Quiz_Api
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

            // for testing
            //  services.AddDbContext<UserIdentityDbContext>(opt => opt.UseInMemoryDatabase("User"));

            //when in prod
            services.AddDbContext<QuizContext>(options => options.UseSqlServer(Configuration.GetConnectionString("CoreUidbconn_quizapi")));
            services.AddDbContext<UserIdentityDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("CoreUidbconn_quizapi")));

            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<UserIdentityDbContext>();


            services.AddCors(options => options.AddPolicy("Cors", builder =>
            {
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            }));


            // using [authorize]
            //using this var userId = HttpContext.User.Claims.First().Value; 
            // using header Name = authentication value = Bearer (key)
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is the secret phrase"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = signingKey,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true
                };
            });

            //add   nuget then here
            // 1. swashbuckle aspnet core and 2. swashbuckle aspet core swagger
            //services.AddSwaggerGen((options) => {
            //    options.SwaggerDoc("v1",
            //        new Info { title = "My Api", version = "v1" });
            //});
            // for swagger using Swashbuckle.AspNetCore.SwaggerGen;
            // then add app.UseSwagger() in cofigure
            services.AddSwaggerGen((options)=>
            {
                options.SwaggerDoc("v1", info: new Microsoft.OpenApi.Models.OpenApiInfo { Title = "v1", Version = "1.0", Description = "Jerome Jose" });
            }
           // options.OperationFilter<SwaggerDefaultValues>()
            );



            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();

            app.UseSwaggerUI(c =>
           {
               c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
           });

            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("Cors");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
