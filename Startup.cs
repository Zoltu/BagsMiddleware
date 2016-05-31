using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using Swashbuckle.SwaggerGen.Generator;
using System.IO;
using BagsMiddleware.Extensions;

namespace Zoltu.BagsMiddleware
{
	public class Startup
	{
		private IConfiguration _configuration { get; set; }
		private IHostingEnvironment _hostingEnvironment { get; set; }

		public Startup(IHostingEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
			// Set up configuration sources.
			_configuration = new ConfigurationBuilder()
				.SetBasePath(_hostingEnvironment.ContentRootPath)
				.AddUserSecrets(_hostingEnvironment)
				.AddApplicationInsightsSettings(developerMode: _hostingEnvironment.IsDevelopment())
				.AddEnvironmentVariables()
				.Build();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			ConfigureCommonServices(services);

			// EntityFramework setup
			services.AddEntityFramework()
				.AddDbContext<Models.BagsContext>(options => options.UseSqlServer(_configuration["SqlServerConnectionString"]));
		}

		// This method gets called by the runtime in development. Use this method to add services to the container.
		public void ConfigureDevelopmentServices(IServiceCollection services)
		{
			ConfigureCommonServices(services);

			// EntityFramework setup
			services.AddEntityFramework()
				.AddDbContext<Models.BagsContext>(options => options.UseSqlite(new SqliteConnection("Data Source=:memory:")));
		}

		private void ConfigureCommonServices(IServiceCollection services)
		{
			// make configuration available as a dependency
			services.AddSingleton(serviceProvider => _configuration);

			// monitoring setup
			services.AddApplicationInsightsTelemetry(_configuration);

			// MVC setup
			services.AddMvc(options =>
			{
				options.RespectBrowserAcceptHeader = true;
				options.OutputFormatters.Insert(0, new HttpNotAcceptableOutputFormatter());
				options.OutputFormatters.RemoveType<StringOutputFormatter>();
			});

			// Swashbuckle setup
			services.AddSwaggerGen(options =>
			{
				options.SingleApiVersion(new Info { Version = "v1", Title = "Zoltu.BagsMiddleware" });
				options.DescribeAllEnumsAsStrings();
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder applicationBuilder, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
		{
			ConfigureCommon(applicationBuilder, hostingEnvironment, loggerFactory);

			// NOTE: this must go at the end of Configure; change when https://github.com/aspnet/Hosting/issues/373 is resolved
			var serviceScopeFactory = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
			using (var serviceScope = serviceScopeFactory.CreateScope())
			{
				var dbContext = serviceScope.ServiceProvider.GetService<Models.BagsContext>();
				dbContext.Database.Migrate();
			}
		}

		public void ConfigureDevelopment(IApplicationBuilder applicationBuilder, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
		{
			ConfigureCommon(applicationBuilder, hostingEnvironment, loggerFactory);

			// NOTE: this must go at the end of Configure; change when https://github.com/aspnet/Hosting/issues/373 is resolved
			var serviceScopeFactory = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
			using (var serviceScope = serviceScopeFactory.CreateScope())
			{
				var dbContext = serviceScope.ServiceProvider.GetService<Models.BagsContext>();
				// intentionally leak this connection so the in-memory database doesn't shut down
				dbContext.Database.OpenConnection();
				dbContext.Database.EnsureDeleted();
				dbContext.Database.EnsureCreated();
			}
		}

		private void ConfigureCommon(IApplicationBuilder applicationBuilder, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(minLevel: LogLevel.Information);

			applicationBuilder.UseApplicationInsightsRequestTelemetry();
			applicationBuilder.UseApplicationInsightsExceptionTelemetry();
			applicationBuilder.UseMvc();

			applicationBuilder.UseSwaggerGen();
			applicationBuilder.UseSwaggerUi();
		}

		// Entry point for the application.
		public static void Main(string[] args) => new WebHostBuilder()
			.UseKestrel()
			.UseContentRoot(Directory.GetCurrentDirectory())
			.UseIISIntegration()
			.UseStartup<Startup>()
			.Build()
			.Run();
	}
}
