using TE_Project.Middleware;

namespace TE_Project.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configures the application to use custom error handling
        /// </summary>
        public static IApplicationBuilder UseCustomErrorHandling(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseMiddleware<ErrorHandlingMiddleware>();
                app.UseHsts();
            }

            return app;
        }

        /// <summary>
        /// Configures the application to use security headers
        /// </summary>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.UseMiddleware<SecurityHeadersMiddleware>();
            return app;
        }

        /// <summary>
        /// Configures the application to use Swagger with custom settings
        /// </summary>
        public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TE Project API v1");
                    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                    c.DisplayRequestDuration();
                    c.EnableFilter();
                });
            }
            
            return app;
        }
    }
}