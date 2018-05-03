using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SecureDemo
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
            services.AddMvc(options => //this applies [ValidateAntiForgeryToken] in whole site smartly to post statements
            {                          // instead of me having to do this for every [HttpPost,put and delete] in controllers
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                options.Filters.Add(new RequireHttpsAttribute()); // force https only on the whole app
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) // add authentication services and use the cookie method...
                .AddCookie(options =>
                {
                    options.AccessDeniedPath = "/Home/ErrorForbidden";
                    options.LoginPath = "/Home/ErrorNotLoggedIn";
                }); // this is a scheme we want to use this as opposed to facebook, etc
                    // what is my federated identity?
                    // 19 minutes in we change .AddCookie to add options. it was just .AddCookie()
                    // before.

            services.AddAuthorization(options => // define admin policy, then apply it in the homecontroller at [authorize]
            {   // add a policy you made up called mustbeadmin and require the user to be authenticated and role to be 'admin'
                options.AddPolicy("MustBeAdmin", p => p.RequireAuthenticatedUser().RequireRole("admin"));

            }); // 22:22 in video we define an authroization policy here
                     
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //
            // security to forward security headers 38:49 of video. load balancer is terminating ssl for you
            //
            //
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });

            //
            // use nwebsec.aspnetcore.middleware package to use security headers for browsers
            // plug into the configure method in startup class to add lightweight middleware that returns
            // security headers

            // stricttransportsecurityheader tells browsers never connect http only https
            app.UseHsts(options => options.MaxAge(days: 365).IncludeSubdomains()); //and use browser whitelist
            // removes TOFU trust on first, always connect https: and get on chrome preload list of sites only allowed
            // on https only:
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXContentTypeOptions(); // close loophole where sites return different media type
            //
            // now go into program.cs to envoke 46:31 to hide server
            app.UseStaticFiles();
            // add security here, above app.UseMvc
            app.UseAuthentication();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
