//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.OpenApi.Models;
//using Swashbuckle.AspNetCore.Filters;
//using Swashbuckle.AspNetCore.SwaggerUI;
//using System.Reflection;

//namespace ZR.Admin.WebApi.Extensions
//{
//    public static class SwaggerExtension
//    {
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="app"></param>
//        public static void UseSwagger(this IApplicationBuilder app)
//        {
//            app.UseSwagger(c =>
//            {
//                c.RouteTemplate = "swagger/{documentName}/swagger.json";
//                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
//                {
//                    var url = $"{httpReq.Scheme}://{httpReq.Host.Value}";
//                    var referer = httpReq.Headers["Referer"].ToString();
//                    if (referer.Contains(GlobalConstant.DevApiProxy))
//                        url = referer[..(referer.IndexOf(GlobalConstant.DevApiProxy, StringComparison.InvariantCulture) + GlobalConstant.DevApiProxy.Length - 1)];
//                    swaggerDoc.Servers =
//                        new List<OpenApiServer>
//                        {
//                            new OpenApiServer
//                            {
//                                Url = url
//                            }
//                        };
//                });
//            });
//            app.UseSwaggerUI(c =>
//            {
//                c.SwaggerEndpoint("sys/swagger.json", "System Management");
//                c.SwaggerEndpoint("article/swagger.json", "Article Management");
//                c.SwaggerEndpoint("v1/swagger.json", "business");
//                c.SwaggerEndpoint("shopping/swagger.json", "Shopping Mall Management");
//                c.DocExpansion(DocExpansion.None); //->Modify interface to auto-collapse when opened
//            });
//        }

//        public static void AddSwaggerConfig(this IServiceCollection services)
//        {
//            if (services == null) throw new ArgumentNullException(nameof(services));
//            //IWebHostEnvironment hostEnvironment = services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>();



//            services.AddSwaggerGen(c =>
//            {
//                c.SwaggerDoc("sys", new OpenApiInfo
//                {
//                    Title = "maham-ai.NET Api",
//                    Version = "v1",
//                    Description = "System Management",
//                    Contact = new OpenApiContact { Name = "maham-ai doc", Url = new Uri("https://www.izhaorui.cn/doc") }
//                });
//                c.SwaggerDoc("article", new OpenApiInfo
//                {
//                    Title = "maham-ai.NET Api",
//                    Version = "v1",
//                    Description = "Article Management",
//                    Contact = new OpenApiContact { Name = "maham-ai doc", Url = new Uri("https://www.izhaorui.cn/doc") }
//                });
//                c.SwaggerDoc("shopping", new OpenApiInfo
//                {
//                    Title = "maham-ai.NET Api",
//                    Version = "v1",
//                    Description = "Shopping Mall Management",
//                });
//                c.SwaggerDoc("v1", new OpenApiInfo
//                {
//                    Title = "maham-ai.NET Api",
//                    Version = "v1",
//                    Description = "",
//                });
//                try
//                {
//                    //var tempPath = hostEnvironment.ContentRootPath;
//                    //Add document comments
//                    var baseDir = AppContext.BaseDirectory;
//                    c.IncludeXmlComments(Path.Combine(baseDir, "ZR.Model.xml"), true);
//                    c.IncludeXmlComments(Path.Combine(baseDir, "ZR.ServiceCore.xml"), true);
//                    c.IncludeXmlComments(Path.Combine(baseDir, "ZR.Service.xml"), true);
//                    c.IncludeXmlComments(Path.Combine(baseDir, "ZR.Admin.WebApi.xml"), true);
//                    //c.IncludeXmlComments(Path.Combine(baseDir, "ZR.Mall.xml"), true);

//                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//                    var xmlPath = Path.Combine(baseDir, xmlFile);
//                    c.IncludeXmlComments(xmlPath);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine("swagger document loading failed" + ex.Message);
//                }

//                //Reference article: http://www.zyiz.net/tech/detail-134965.html
//                //Need to install package Swashbuckle.AspNetCore.Filters
//                // Enable permission lock - need to add [Authorize] on corresponding Action to see it
//                c.OperationFilter<AddResponseHeadersFilter>();
//                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

//                //Add token in header, pass to backend
//                c.OperationFilter<SecurityRequirementsOperationFilter>();

//                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
//                    new OpenApiSecurityScheme
//                    {
//                        In = ParameterLocation.Header,
//                        Description = "Please enter the Token returned by the Login interface, prefixed with Bearer. Example: Bearer {Token}",
//                        Name = "Authorization",//jwt default parameter name,
//                        Type = SecuritySchemeType.ApiKey, //Specify ApiKey
//                        BearerFormat = "JWT",//Identifies the format of the bearer token. This information is mainly for documentation purposes
//                        Scheme = JwtBearerDefaults.AuthenticationScheme//Name of the HTTP authorization scheme to be used in authorization
//                    });
//                c.AddSecurityRequirement(new OpenApiSecurityRequirement
//                {
//                    {
//                        new OpenApiSecurityScheme
//                        {
//                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
//                        },
//                        new List<string>()
//                    }
//                });

//                try
//                {
//                    //Determine which group the interface belongs to
//                    c.DocInclusionPredicate((docName, apiDescription) =>
//                    {
//                        if (docName == "v1")
//                        {
//                            //When the group is NoGroup, all without attributes belong to this group
//                            return string.IsNullOrEmpty(apiDescription.GroupName);
//                        }
//                        else
//                        {
//                            return apiDescription.GroupName == docName;
//                        }
//                    });
//                }
//                catch (Exception e)
//                {
//                    Console.WriteLine(e);
//                    throw;
//                }

//            });
//        }
//    }
//}











using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace ZR.Admin.WebApi.Extensions
{
    public static class SwaggerExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        public static void UseSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    var url = $"{httpReq.Scheme}://{httpReq.Host.Value}";
                    var referer = httpReq.Headers["Referer"].ToString();
                    if (referer.Contains(GlobalConstant.DevApiProxy))
                        url = referer[..(referer.IndexOf(GlobalConstant.DevApiProxy, StringComparison.InvariantCulture) + GlobalConstant.DevApiProxy.Length - 1)];
                    swaggerDoc.Servers =
                        new List<OpenApiServer>
                        {
                            new OpenApiServer
                            {
                                Url = url
                            }
                        };
                });
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("sys/swagger.json", "System Management");
                c.SwaggerEndpoint("article/swagger.json", "Article Management");
                c.SwaggerEndpoint("v1/swagger.json", "Business Management");
                c.SwaggerEndpoint("shopping/swagger.json", "Mall Management");
                c.SwaggerEndpoint("hr/swagger.json", "HR Management");
                c.DocExpansion(DocExpansion.None); //->Modify interface to auto-collapse when opened
                c.DefaultModelsExpandDepth(-1); // Hide schemas section by default
                c.DisplayRequestDuration(); // Show request duration
                c.EnableDeepLinking(); // Enable deep linking
                c.EnableFilter(); // Enable filter box
            });
        }

        public static void AddSwaggerConfig(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            //IWebHostEnvironment hostEnvironment = services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("sys", new OpenApiInfo
                {
                    Title = "maham-ai.NET Api",
                    Version = "v1",
                    Description = "System Management",
                    Contact = new OpenApiContact { Name = "maham-ai doc", Url = new Uri("https://www.izhaorui.cn/doc") }
                });
                c.SwaggerDoc("article", new OpenApiInfo
                {
                    Title = "maham-ai.NET Api",
                    Version = "v1",
                    Description = "Article Management",
                    Contact = new OpenApiContact { Name = "maham-ai doc", Url = new Uri("https://www.izhaorui.cn/doc") }
                });
                c.SwaggerDoc("shopping", new OpenApiInfo
                {
                    Title = "maham-ai.NET Api",
                    Version = "v1",
                    Description = "Mall Management",
                });
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "maham-ai.NET Api",
                    Version = "v1",
                    Description = "Business Management API",
                });
                //c.SwaggerDoc("hr", new OpenApiInfo
                //{
                //    Title = "maham-ai.NET Api",
                //    Version = "v1",
                //    Description = "HR Management API",
                //    Contact = new OpenApiContact { Name = "HR Management", Email = "hr@example.com" }
                //});
                try
                {
                    //var tempPath = hostEnvironment.ContentRootPath;
                    //Add document comments
                    var baseDir = AppContext.BaseDirectory;
                    c.IncludeXmlComments(Path.Combine(baseDir, "ZR.Model.xml"), true);
                    c.IncludeXmlComments(Path.Combine(baseDir, "ZR.ServiceCore.xml"), true);
                    c.IncludeXmlComments(Path.Combine(baseDir, "ZR.Service.xml"), true);
                    c.IncludeXmlComments(Path.Combine(baseDir, "ZR.Admin.WebApi.xml"), true);
                    //c.IncludeXmlComments(Path.Combine(baseDir, "ZR.Mall.xml"), true);

                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(baseDir, xmlFile);
                    c.IncludeXmlComments(xmlPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("swagger Document loading failed" + ex.Message);
                }

                //Reference article: http://www.zyiz.net/tech/detail-134965.html
                //Need to install package Swashbuckle.AspNetCore.Filters
                // Enable permission lock - need to add [Authorize] on corresponding Action to see it
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                //Add token in header, pass to backend
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter the token returned by the Login interface and set the Bearer in the front. Example: Bearer {Token}",
                        Name = "Authorization",//jwt default parameter name,
                        Type = SecuritySchemeType.ApiKey, //Specify ApiKey
                        BearerFormat = "JWT",//Identifies the format of the bearer token. This information is mainly for documentation purposes
                        Scheme = JwtBearerDefaults.AuthenticationScheme//Name of the HTTP authorization scheme to be used in authorization
                    });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new List<string>()
                    }
                });

                try
                {
                    //Determine which group the interface belongs to
                    c.DocInclusionPredicate((docName, apiDescription) =>
                    {
                        if (docName == "v1")
                        {
                            //When the group is NoGroup, all without attributes belong to this group
                            return string.IsNullOrEmpty(apiDescription.GroupName);
                        }
                        else
                        {
                            return apiDescription.GroupName == docName;
                        }
                    });
                    
                    // Custom operation ID to avoid conflicts
                    c.CustomOperationIds(apiDescription =>
                    {
                        var controllerName = apiDescription.ActionDescriptor.RouteValues["controller"];
                        var actionName = apiDescription.ActionDescriptor.RouteValues["action"];
                        var method = apiDescription.HttpMethod?.ToLower() ?? "get";
                        return $"{controllerName}_{actionName}_{method}";
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                // Handle conflicting actions - choose the first one and log warning
                c.ResolveConflictingActions(apiDescriptions => 
                {
                    var descriptions = apiDescriptions.ToList();
                    if (descriptions.Count > 1)
                    {
                        Console.WriteLine($"Warning: Conflicting actions detected for {descriptions.First().RelativePath}");
                        foreach (var desc in descriptions)
                        {
                            Console.WriteLine($"  - {desc.ActionDescriptor.DisplayName}");
                        }
                    }
                    return descriptions.First();
                });
                
                // Add custom schema IDs to avoid conflicts
                c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
                
                // Enable annotations
                c.EnableAnnotations();
            });
        }
    }
}











