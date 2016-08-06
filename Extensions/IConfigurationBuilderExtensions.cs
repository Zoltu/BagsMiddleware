using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Zoltu.Bags.Api.Extensions
{
	public static class IConfigurationBuilderExtensions
	{
		public static IConfigurationBuilder AddUserSecrets(this IConfigurationBuilder it, IHostingEnvironment hostingEnvironment) => hostingEnvironment.IsDevelopment() ? it.AddUserSecrets() : it;
	}
}
