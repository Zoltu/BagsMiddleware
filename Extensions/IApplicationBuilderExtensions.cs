using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Zoltu.Bags.Api.Extensions
{
	public static class IApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseApplicationInsightsInitializer(this IApplicationBuilder applicationBuilder, ITelemetryInitializer telemetryInitializer)
		{
			applicationBuilder.ApplicationServices.GetRequiredService<TelemetryConfiguration>().TelemetryInitializers.Add(telemetryInitializer);
			return applicationBuilder;
		}
    }
}
