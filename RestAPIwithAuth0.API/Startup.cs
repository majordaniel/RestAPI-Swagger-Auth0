using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RestAPIwithAuth0.Business.Helpers.Abstracts;
using RestAPIwithAuth0.Business.Implementations;
using RestAPIwithAuth0.Business.Interfaces;
using RestAPIwithAuth0.Data.Configs;

namespace RestAPIwithAuth0.API
{

    public class Startup
    {
        //private string ContentRootPath;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                   builder
                   .WithOrigins(Configuration["FrontendOrigin"])
                   .WithHeaders("Content-Type", "Authorization")
                   .AllowAnyMethod()
                   .AllowCredentials()
                );

                options.AddPolicy("UnsafeCORSAllow", builder =>
                   builder
                   .WithHeaders("Content-Type", "Authorization")
                   .AllowAnyMethod()
                   .AllowAnyOrigin()
                );
            });


            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                   options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );

            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(
                   Configuration["ConnectionStrings:DefaultConnection"]
               )
            );


            services.AddAppServices(Configuration);

            services.AddAutoMapper(typeof(AutomapperProfile));

            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RESTAPI", Version = "v1" });
            //    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            //    {
            //        Type = SecuritySchemeType.OAuth2,
            //        Flows = new OpenApiOAuthFlows
            //        {
            //            Implicit = new OpenApiOAuthFlow
            //            {
            //                AuthorizationUrl = new Uri("herotech.us.auth0.com/oauth/token", UriKind.Relative),
            //                Scopes = new Dictionary<string, string>
            //                {
            //                    { "readAccess", "Access read operations" },
            //                    { "writeAccess", "Access write operations" }
            //                }
            //            }
            //        }
            //    });

            //    c.OperationFilter<OAuth2OperationFilter>();

            //    //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //    //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            //    //c.IncludeXmlComments(xmlPath);
            //});

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = $"https://{Configuration["Auth0:Domain"]}/";
                // options.Authority = "https://herotechng.us.auth0.com/";
                options.Audience = Configuration["Auth0:Audience"];
                // options.Audience = "https://localhost:44386/";
            });


            services.AddSwaggerGen(c =>
            {
                //c.SwaggerDoc("API", new OpenApiInfo()
                //{
                //    Version = "v2",
                //    Title = "My API",
                //});
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RESTAPI", Version = "v1" });

                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Implicit = new OpenApiOAuthFlow()
                        {
                            //  Configuration["ConnectionStrings:DefaultConnection"]
                            //  AuthorizationUrl = new Uri($"{Configuration["Auth0:Domain"]}/authorize?audience={System.Web.HttpUtility.UrlEncode(Configuration["Auth0:Audience"])}", UriKind.Absolute),
                            AuthorizationUrl = new Uri($"https://{Configuration["Auth0:Domain"]}/authorize?audience={System.Web.HttpUtility.UrlEncode(Configuration["Auth0:Audience"])}", UriKind.Absolute),
                            // AuthorizationUrl = new Uri("https://herotechng.us.auth0.com/authorize?audience=https://localhost:44386/", UriKind.Absolute),

                            Scopes = new Dictionary<string, string>
                                            {
                                                { "readAccess", "Access read operations" },
                                                { "writeAccess", "Access write operations" }
                                            }

                        },
                        //ClientCredentials = new OpenApiOAuthFlow()
                        //{
                        //    TokenUrl = new Uri($"https://{Configuration["Auth0:Domain"]}/oauth/token?audience={System.Web.HttpUtility.UrlEncode(Configuration["Auth0:Audience"])}", UriKind.Absolute),
                        //   // TokenUrl = new Uri("https://herotechng.us.auth0.com/oauth/token?audience=https://localhost:44386/", UriKind.Absolute),
                        //},
                    },
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                  {
                      {
                          new OpenApiSecurityScheme()
                          {
                              Reference = new OpenApiReference()
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "oauth2",
                              },
                          },
                          Enumerable.Empty<string>().ToList()
                      },
                  });

            });




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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();

            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestAPIService");
            //    c.OAuthClientId("qKIFpG5g66hlXIoyM5tr7W2jBEKx4Ji6");
            //    c.OAuthClientSecret("iy1TzVew_N5PrkeyeFBNXq_GdNli94HeiTYIy2XRKCvLlM1WlIttGp9i4jkixrmg");
            //    c.OAuthRealm("client-realm");
            //    c.OAuthAppName("EmployeesDirectory");


            //    c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            //});

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestAPIService");

                //  c.OAuthClientId(Configuration["Swagger:ClientId"]);

                // c.OAuthClientId("nNouWDtODw50tjcg2yvaZN80XliNDJbh");
                // c.OAuthClientSecret("iy1TzVew_N5PrkeyeFBNXq_GdNli94HeiTYIy2XRKCvLlM1WlIttGp9i4jkixrmg");

                c.OAuthClientId($"{Configuration["Auth0:ClientId"]}");
                c.OAuthClientSecret($"{Configuration["Auth0:ClientSecret"]}");

                c.InjectJavascript("/Auth0.js");
            });
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
