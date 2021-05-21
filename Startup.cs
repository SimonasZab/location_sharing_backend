using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIUtils;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using location_sharing_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace location_sharing_backend
{
	public class Startup
	{
		//readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			/*services.AddCors(options =>
			{
				options.AddPolicy(name: MyAllowSpecificOrigins,
					builder => {
						builder.WithOrigins("https://localhost:8080", "http://localhost:8080").AllowCredentials().AllowAnyHeader();
					});
			});*/

			services.AddAuthentication(
				options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				}
			).AddJwtBearer(jwt =>
			{
				var key = Assets.Secrets.JWTSecret;

				jwt.SaveToken = true;
				jwt.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false,
					RequireExpirationTime = false,
					ValidateLifetime = true
				};
				jwt.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						context.Token = context.Request.Cookies[Assets.Secrets.AccessTokenCookieName];
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
			services.AddSingleton<MailSender>();

			services.AddControllers(
				options =>
				{
					options.Filters.Add(new APIExceptionFilter());
					options.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
				}
			).AddNewtonsoftJson();

			/*services.AddSwaggerGen(c => {
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "location_sharing_backend", Version = "v1" });
			});*/
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				//app.UseDeveloperExceptionPage();
				//app.UseSwagger();
				//app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "location_sharing_backend v1"));
			}

			//app.UseHttpsRedirection();

			var cookiePolicyOptions = new CookiePolicyOptions
			{
				Secure = CookieSecurePolicy.Always,
				MinimumSameSitePolicy = SameSiteMode.None,
				HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always
			};
			app.UseCookiePolicy(cookiePolicyOptions);

			app.UseRouting();

			//app.UseCors(MyAllowSpecificOrigins);

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
		{
			var builder = new ServiceCollection()
				.AddLogging()
				.AddMvc()
				.AddNewtonsoftJson()
				.Services.BuildServiceProvider();

			return builder
				.GetRequiredService<IOptions<MvcOptions>>()
				.Value
				.InputFormatters
				.OfType<NewtonsoftJsonPatchInputFormatter>()
				.First();
		}
	}
}
