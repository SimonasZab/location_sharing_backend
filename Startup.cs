using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Api.Backends;

namespace Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAuthentication(
				options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				}
			).AddJwtBearer(jwt =>
			{
				jwt.SaveToken = true;
				jwt.TokenValidationParameters = AuthBackend.TokenValidationParameters;
				jwt.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						Endpoint? endpoint = context.HttpContext.GetEndpoint();
						if (endpoint == null) {
							return Task.CompletedTask;
						}
						if (endpoint.Metadata.GetMetadata<AuthorizeAttribute>() != null) {
							context.Token = context.Request.Cookies[Assets.Secrets.AccessTokenCookieName];
						}
						return Task.CompletedTask;
					}
				};
			});

			services.AddSingleton<UserService>();
			services.AddSingleton<UserBlockService>();
			services.AddSingleton<ConnectionService>();
			services.AddSingleton<LocationService>();
			services.AddSingleton<UserShareService>();
			services.AddSingleton<UserVerificationService>();

			services.AddControllers(
				options =>
				{
					options.Filters.Add(new APIExceptionFilter());
				}
			);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			/*if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}*/

			var cookiePolicyOptions = new CookiePolicyOptions
			{
				Secure = CookieSecurePolicy.Always,
				MinimumSameSitePolicy = SameSiteMode.None,
				HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always
			};
			app.UseCookiePolicy(cookiePolicyOptions);

			app.UsePathBase(new PathString(Assets.Config.PathBase));
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
