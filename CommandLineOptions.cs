using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace InstaFeed
{
	public class CommandLineOptions : CommandLineOptionsBase
	{
		public CommandLineOptions()
		{
			CommandsList = new List<string>();
			IncludeDirectories = new List<string>();
		}

		public enum Commands
		{
			list, rss, archive
		}

		[OptionList("c", "command", Separator = ',', Required = true, HelpText = "Specifies command or comma separated list of commands: list|rss|archive.")]
		public List<string> CommandsList { get; set; }

		[Option("u", "username", Required = true, HelpText = "Specifies Instapaper account username.")]
		public string Username { get; set; }

		[Option("p", "password", Required = true, HelpText = "Specifies Instapaper account password.")]
		public string Password { get; set; }

		[OptionList("d", "directories", Separator = ',', HelpText = "Comma separated list of directories to include. Names case insensitive. If not specified all directories are processed.")]
		public List<string> IncludeDirectories { get; set; }

		[Option("o", "output", DefaultValue = ".\\feed.rss", HelpText = "Specifies output file name and destination for RSS feed. Equal to .\\feed.rss by default. Can contain environment variables.")]
		public string OutputFileName { get; set; }

		[HelpOption("h", "help")]
		public string GetUsage() { return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current)); }
	}
}