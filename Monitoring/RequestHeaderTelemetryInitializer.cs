using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace BagsMiddleware.Monitoring
{
	public class RequestHeaderTelemetryInitializer : ITelemetryInitializer
	{
		private TelemetryClient _telemetryClient = new TelemetryClient();
		private IHttpContextAccessor _httpContextAccessor;

		public RequestHeaderTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public void Initialize(ITelemetry telemetry)
		{
			try
			{
				var headers = _httpContextAccessor.HttpContext?.Request?.Headers ?? new HeaderDictionary(0);

				foreach (var header in headers)
				{
					telemetry.Context.Properties.Add(new KeyValuePair<String, String>($"Header: {header.Key}", header.Value));
				}
			}
			catch (Exception exception)
			{
				_telemetryClient.TrackException(exception);
			}
		}
	}
}
