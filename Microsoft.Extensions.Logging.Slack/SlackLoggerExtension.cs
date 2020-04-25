using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;


namespace Microsoft.Extensions.Logging.Slack
{
	public static class SlackLoggerExtension
	{
		public static ILoggerFactory AddSlack(this ILoggerFactory factory, SlackConfiguration configuration, 
			string applicationName, string environmentName, HttpClient client = null)
		{
			//dee : reduce code repeate
			return AddSlack(factory, null, configuration, applicationName, environmentName, client);
		}

		public static ILoggerFactory AddSlack(this ILoggerFactory factory, SlackConfiguration configuration, IHostEnvironment hostingEnvironment, HttpClient client = null)
		{
			return AddSlack(factory, null, configuration, hostingEnvironment.ApplicationName, hostingEnvironment.EnvironmentName, client);
		}

		public static ILoggerFactory AddSlack(this ILoggerFactory factory, Func<string, LogLevel, Exception, bool> filter, SlackConfiguration configuration, IHostEnvironment hostingEnvironment, HttpClient client = null)
		{
			return AddSlack(factory, filter, configuration, hostingEnvironment.ApplicationName, hostingEnvironment.EnvironmentName, client);
		}

		public static ILoggerFactory AddSlack(this ILoggerFactory factory, Func<string, LogLevel, Exception, bool> filter, SlackConfiguration configuration, 
			string applicationName, string environmentName, HttpClient client = null)
		{
			//dee for legacy support
			if (null != filter)
			{
				configuration.filter = (s, l, e) => filter(s, l, e);
			}

			if (string.IsNullOrEmpty(applicationName))
			{
				throw new ArgumentNullException(nameof(applicationName));
			}

			if (string.IsNullOrEmpty(environmentName))
			{
				throw new ArgumentNullException(nameof(environmentName));
			}

			ILoggerProvider provider = new SlackLoggerProvider(configuration, client, applicationName, environmentName);

			factory.AddProvider(provider);

			return factory;
		}



	}
}