using System;

namespace Microsoft.Extensions.Logging.Slack
{
	public delegate bool CustomFilter(string CategoryName, LogLevel level, Exception ex);
	
	/// <summary>
	/// Methods to apply custom formatting
	/// </summary>
	/// <param name="logText"></param>
	/// <param name="CategoryName"></param>
	/// <param name="level"></param>
	/// <param name="ex"></param>
	/// <returns>A Json object to be serialized and sent to Slack</returns>
	public delegate object SlackFormater(string logText, string CategoryName, LogLevel level, Exception ex);

	public class SlackConfiguration
	{
		/// <summary>
		/// The Slack API Uri 
		/// </summary>
		public Uri webhookUrl { get; set; }
		
		/// <summary>
		/// Minimum level to log
		/// </summary>
		public LogLevel MinLevel { get; set; }

		/// <summary>
		/// OPTIONAL Custom filter method. Will override MinLevel
		/// </summary>
		public CustomFilter filter { get; set; }

		/// <summary>
		/// OPTIONAL custom formatter Use https://api.slack.com/tools/block-kit-builder?mode=message&blocks=%5B%5D
		/// </summary>
		public SlackFormater slackFormatter { get; set; }


		public SlackConfiguration()
		{
			MinLevel = LogLevel.Information;
		}
	}
}