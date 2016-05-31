using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace BagsMiddleware.Extensions
{
	public static class IConfigurationBuilderExtensions
    {
		public static IConfigurationBuilder AddUserSecrets(this IConfigurationBuilder it, IHostingEnvironment hostingEnvironment) => hostingEnvironment.IsDevelopment() ? it.AddUserSecrets() : it;
    }
}
