using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.SwaggerGen;

namespace Templates.ApiApp
{
	public class Startup
	{
		public IConfigurationRoot Configuration { get; set; }

		public Startup(IHostingEnvironment hostingEnvironment)
		{
			// Set up configuration sources.
			Configuration = new ConfigurationBuilder()
				.AddEnvironmentVariables()
				.Build();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
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
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder applicationBuilder, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
		{
			applicationBuilder.UseMvc();

			applicationBuilder.UseSwaggerGen();
			applicationBuilder.UseSwaggerUi();
		}

		// Entry point for the application.
		public static void Main(string[] args) => Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);
	}
}
