using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Extensions.Logging.Slack
{
	public class SlackLogger : ILogger
	{
		private readonly HttpClient httpClient;
		private readonly string applicationName;
		private readonly string environmentName;
		readonly SlackConfiguration _configuration;
		readonly string _categoryName;

		public SlackLogger(string categoryName, 
									HttpClient httpClient, 
									string environmentName, 
									string applicationName,
									SlackConfiguration configuration)
		{
			this.environmentName = environmentName;
			this.applicationName = applicationName;
			_configuration = configuration;
			_categoryName = categoryName;
			this.httpClient = httpClient;
		}

		/// <summary>
		/// Writes a log entry.
		/// </summary>
		/// <param name="logLevel">Entry will be written on this level.</param>
		/// <param name="eventId">Id of the event.</param>
		/// <param name="state">The entry to be written. Can be also an object.</param>
		/// <param name="exception">The exception related to this entry.</param>
		/// <param name="formatter">Function to create a <c>string</c> message of the <paramref name="state"/> and <paramref name="exception"/>.</param>
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel, exception))
			{
				return;
			}

			if (formatter == null)
			{
				throw new ArgumentNullException(nameof(formatter));
			}

			var title = formatter(state, exception);

			var color = "good";

			switch (logLevel)
			{
				case LogLevel.None:
				case LogLevel.Trace:
				case LogLevel.Debug:
				case LogLevel.Information:
					color = "good";
					break;
				case LogLevel.Warning:
					color = "warning";
					break;
				case LogLevel.Error:
				case LogLevel.Critical:
					color = "danger";
					break;
			}

			var logObj = null != _configuration.slackFormatter? _configuration.slackFormatter(title,_categoryName, logLevel, exception) : new
			{
				attachments = new[]
				{
					new
					{
						fallback = $"[{applicationName}] [{environmentName}] [{_categoryName}] [{title}].",
						color,
						title = $"v1.4 : {title}",
						text = exception?.ToString(),
						fields = new[]
						{
							new
							{
								title = "Project",
								value = applicationName,
								@short = "true"
							},
							new
							{
								title = "Environment",
								value = environmentName,
								@short = "true"
							}
						}
					}
				}
			};

			var jsondata = JsonConvert.SerializeObject(logObj);

			try
			{
				var posted = httpClient.PostAsync(_configuration.webhookUrl, new StringContent(jsondata, Encoding.UTF8, "application/json")).Result;

				if (!posted.IsSuccessStatusCode)
				{
					Console.Error.WriteLine($"failed json: {jsondata}");

					var error = $"crit: Failed with Status code {posted.StatusCode} : ";
					try
					{
						error += posted.Content.ReadAsStringAsync().Result;
					}
					catch { }
					Console.Error.WriteLine(error);
				}
			}
			catch(Exception ex)
			{
				throw new Exception($"failed to post to slack url -> {_configuration.webhookUrl}, data -> {jsondata}", ex);
			}
		}

		/// <summary>
		/// Checks if the given <paramref name="logLevel"/> is enabled.
		/// </summary>
		/// <param name="logLevel">level to be checked.</param>
		/// <returns><c>true</c> if enabled.</returns>
		public bool IsEnabled(LogLevel logLevel)
		{
			return IsEnabled(logLevel, null);
		}

		public bool IsEnabled(LogLevel logLevel, Exception exc)
		{
			return null == _configuration.filter ? logLevel >= _configuration.MinLevel : _configuration.filter(_categoryName, logLevel, exc);
		}

		/// <summary>
		/// Begins a logical operation scope.
		/// </summary>
		/// <param name="state">The identifier for the scope.</param>
		/// <returns>An IDisposable that ends the logical operation scope on dispose.</returns>
		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}
	}
}