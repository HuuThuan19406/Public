using Api.Entities;
using Api.Models;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Api
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
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder
                            .WithOrigins("https://localhost:44304", "https://nonssl.bestsv.net", "http://localhost:3006", "https://localhost:3006", "https://localhost:44350", "https://editor.swagger.io", "https://api.bestsv.net", "https://admin.bestsv.net")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            services.AddScoped<BestsvContext>();

            services.AddControllersWithViews().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            services.AddOptions();
            services.AddMemoryCache();

            services.Configure<IpRateLimitOptions>(options =>
            {
                options.EnableEndpointRateLimiting = true;
                options.StackBlockedRequests = false;
                options.RealIpHeader = "X-Real-IP";
                options.ClientIdHeader = "X-ClientId";
                options.HttpStatusCode = StatusCodes.Status429TooManyRequests;

                options.GeneralRules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        PeriodTimespan = TimeSpan.FromSeconds(30),
                        Limit = 75
                    },
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        PeriodTimespan = TimeSpan.FromMinutes(20),
                        Limit = 2000
                    }
                };
            });

            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            services.AddMvc();

            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<BestsvContext>();            

            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var result = new StatusError
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Lỗi dữ liệu không mong muốn.",
                            RepairGuides = context.ModelState.Values.SelectMany(m => m.Errors.Select(e => e.ErrorMessage))
                        }.Result();

                        return result;
                    };
                });

            services.AddControllers();

            services
            .AddAuthentication(p =>
            {
                p.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                p.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                p.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(p =>
           {
               p.RequireHttpsMetadata = false;
               p.SaveToken = true;
               p.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(Authenticator.SECRET),
                   ValidIssuers = new string[] { "api.bestsv.net", "localhost:44370" },
                   ValidAudiences = new string[] { "api.bestsv.net", "localhost:44370" },
                   ClockSkew = TimeSpan.Zero
               };
           });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(12);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BestsvApi",
                    Version = "v1",
                    Description = "Api của Bestsv.<br/>Để đảm bảo tài nguyên hệ thống, api được giới hạn mức truy cập tối đa như sau: <b>75 lượt mỗi 30 giây, 2.000 lượt mỗi 20 phút</b>.<br/>Phân nhánh của api được chia theo quyền của Tài Khoản, cụ thể: <b>public:</b> không cần xác thực; <b>user</b>: chỉ cần xác thực; <b>business</b>: cần quyền <i>supplier</i>; <b>admin</b>: cần quyền <i>administrator</i>.",
                    Contact = new OpenApiContact
                    {
                        Name = "Nhóm Kỹ Thuật",
                        Email = "dev@bestsv.net"
                    }
                });

                c.AddServer(new OpenApiServer { Url = "https://localhost:44370" });
                c.AddServer(new OpenApiServer { Url = "https://api.bestsv.net" });

                c.IncludeXmlComments("Api.xml");

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                    }
                });
            });

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
                options.ExcludedHosts.Add("api.bestsv.net");
                options.ExcludedHosts.Add("www.api.bestsv.net");
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
                options.HttpsPort = 5001;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestApi v1"));
            }          

            app.UseHttpsRedirection();

            app.UseHsts();

            app.UseIpRateLimiting();            

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}