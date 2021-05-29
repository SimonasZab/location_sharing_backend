using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Api
{
	public class Program
	{
		public static void Main(string[] args)
		{
			new Assets();
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(
					configurationBuilder => { }
				).ConfigureWebHostDefaults(
					webBuilder =>
						{
							webBuilder.UseStartup<Startup>();
						}
				);
	}
}
