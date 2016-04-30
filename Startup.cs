using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.SwaggerGen;

namespace Zoltu.BagsMiddleware
{
	public class Startup
	{
		private IConfigurationRoot _configuration { get; set; }
		private IHostingEnvironment _hostingEnvironment { get; set; }

		public Startup(IHostingEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
			// Set up configuration sources.
			_configuration = new ConfigurationBuilder()
				.AddUserSecrets()
				.AddApplicationInsightsSettings(developerMode: _hostingEnvironment.IsDevelopment())
				.AddEnvironmentVariables()
				.Build();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
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
			services.AddSwaggerGen();
			services.ConfigureSwaggerDocument(options => options.SingleApiVersion(new Info { Version = "v1", Title = "Zoltu.BagsMiddleware" }));
			services.ConfigureSwaggerSchema(options => options.DescribeAllEnumsAsStrings = true);

			// EntityFramework setup
			if (_hostingEnvironment.IsDevelopment())
				services.AddEntityFramework()
					.AddInMemoryDatabase()
					.AddDbContext<Models.BagsContext>(options => options.UseInMemoryDatabase());
			else
				services.AddEntityFramework()
					.AddSqlServer()
					.AddDbContext<Models.BagsContext>(options => options.UseSqlServer(_configuration["SqlServerConnectionString"]));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder applicationBuilder, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(minLevel: LogLevel.Information);

			applicationBuilder.UseApplicationInsightsRequestTelemetry();
			applicationBuilder.UseApplicationInsightsExceptionTelemetry();
			applicationBuilder.UseMvc();

			applicationBuilder.UseSwaggerGen();
			applicationBuilder.UseSwaggerUi();

			// NOTE: this must go at the end of Configure; change when https://github.com/aspnet/Hosting/issues/373 is resolved
			var serviceScopeFactory = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
			using (var serviceScope = serviceScopeFactory.CreateScope())
			{
				var dbContext = serviceScope.ServiceProvider.GetService<Models.BagsContext>();
				dbContext.Database.EnsureCreated();
			}
		}

		// Entry point for the application.
		public static void Main(string[] args) => Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);
	}
}
