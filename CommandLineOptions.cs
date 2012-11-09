using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace InstaFeed
{
	public class CommandLineOptions : CommandLineOptionsBase
	{
		public enum Commands
		{
			list, rss, archive
		}

		[Option("c", "command", Required = true, DefaultValue = Commands.list, HelpText = "Specifies command: list|rss|archive'")]
		public Commands Command { get; set; }

		[Option("u", "username", Required = true, HelpText = "Instapaper account username")]
		public string Username { get; set; }
		
		[Option("p", "password", Required = true, HelpText = "Instapaper account password")]
		public string Password { get; set; }

		[OptionList("d", "directories", Separator = ',', HelpText = "Comma separated list of directories to include. Names are case insensitive.")]
		public List<string> IncludeDirectories { get; set; }

		[Option("o", "output", DefaultValue = ".\\feed.rss", HelpText = "Specifies output file name and destination for RSS feed. Can contain environment variables.")]
		public string OutputFileName { get; set; }

		[HelpOption("h", "help")]
		public string GetUsage() { return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current)); }
	}
}