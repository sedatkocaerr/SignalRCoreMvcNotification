using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SignalRCoreMvcNotification.AutoMapper;
using SignalRCoreMvcNotification.DataContext;
using SignalRCoreMvcNotification.Models;
using SignalRCoreMvcNotification.Redis;
using SignalRCoreMvcNotification.Security;
using SignalRCoreMvcNotification.Security.Jwt;
using System;
using System.Text;

namespace SignalRCoreMvcNotification
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
            services.AddControllersWithViews();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.AddSignalR();
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = "SignalRCoreMvcNotification";
                options.Configuration = Configuration.GetConnectionString("Redis");
            });
            services.AddDbContext<SignalRCoreDataContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IPasswordHash, PasswordHash>();
            services.AddScoped<IRedisService, RedisManager>();

            services.AddAutoMapper(typeof(ModelToViewModelMapping), typeof(ViewModelToModelMapping));

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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseMiddleware<JwtMiddleware>();
            app.UseCors();
            app.UseAuthorization();
            app.UseAuthentication();
            app.UseCookiePolicy();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<UsersHub>("/usershub");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
