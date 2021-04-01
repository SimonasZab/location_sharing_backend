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
using location_sharing_backend.Models;
using location_sharing_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace location_sharing_backend {
	public class Startup {
		//readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services) {
			/*services.AddCors(options =>
			{
				options.AddPolicy(name: MyAllowSpecificOrigins,
					builder => {
						builder.WithOrigins("https://localhost:8080", "http://localhost:8080").AllowCredentials().AllowAnyHeader();
					});
			});*/

			var secrets = Configuration.GetSection(nameof(Secrets)).Get<Secrets>();

			services.AddAuthentication(options => {
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(jwt => {
				var key = secrets.JWTSecret;

				jwt.SaveToken = true;
				jwt.TokenValidationParameters = new TokenValidationParameters {
					ValidateIssuerSigningKey = true, // this will validate the 3rd part of the jwt token using the secret that we added in the appsettings and verify we have generated the jwt token
					IssuerSigningKey = new SymmetricSecurityKey(key), // Add the secret key to our Jwt encryption
					ValidateIssuer = false,
					ValidateAudience = false,
					RequireExpirationTime = false,
					ValidateLifetime = true
				};
				jwt.Events = new JwtBearerEvents {
					OnMessageReceived = context => {
						context.Token = context.Request.Cookies[secrets.AccessTokenCookieName];
						return Task.CompletedTask;
					}
				};
			});

			services.Configure<Secrets>(Configuration.GetSection(nameof(Secrets)));
			services.AddSingleton<ISecrets>(sp => sp.GetRequiredService<IOptions<Secrets>>().Value);

			services.Configure<DatabaseSettings>(Configuration.GetSection(nameof(Secrets)).GetSection(nameof(DatabaseSettings)));
			services.AddSingleton<IDatabaseSettings>(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);

			services.AddSingleton<UserService>();
			services.AddSingleton<UserBlockService>();
			services.AddSingleton<ConnectionService>();
			services.AddSingleton<LocationService>();
			services.AddSingleton<UserShareService>();

			services.AddControllers(
				options => {
					options.Filters.Add(new APIExceptionFilter());
					options.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
				}
			).AddNewtonsoftJson();

			/*services.AddSwaggerGen(c => {
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "location_sharing_backend", Version = "v1" });
			});*/
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment()) {
				//app.UseDeveloperExceptionPage();
				//app.UseSwagger();
				//app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "location_sharing_backend v1"));
			}

			//app.UseHttpsRedirection();

			var cookiePolicyOptions = new CookiePolicyOptions {
				Secure = CookieSecurePolicy.Always,
				MinimumSameSitePolicy = SameSiteMode.Strict,
				HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always
			};
			app.UseCookiePolicy(cookiePolicyOptions);

			app.UseRouting();

			//app.UseCors(MyAllowSpecificOrigins);

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
			});
		}

		private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter() {
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
