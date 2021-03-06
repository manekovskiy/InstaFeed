﻿using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using CommandLine;

namespace InstaFeed
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var options = new CommandLineOptions();
			var parser = new CommandLineParser(new CommandLineParserSettings(
				false /*case sensitive*/,
				false /*mutually exclusive*/,
				true  /*ignore unknown arguments*/,
				Console.Error));

			if (!parser.ParseArguments(args, options))
				Environment.Exit(1);

			Instapaper feeder = new Instapaper
			{
				Username = options.Username,
				Password = options.Password
			};
			feeder.OnMessage += (_, e) => Console.WriteLine(e.Message);

			foreach (var commandString in options.CommandsList)
			{
				CommandLineOptions.Commands command;
				if(!Enum.TryParse(commandString, true /*ignore case*/, out command))
					continue;

				switch (command)
				{
					case CommandLineOptions.Commands.list:
						{
							feeder.Directories.ForEach(d => Console.WriteLine(d.Name));

							break;
						}
					case CommandLineOptions.Commands.rss:
						{
							var feedItems = (options.IncludeDirectories.Count == 0 
								? feeder.Directories
								: feeder.Directories.Where(directory => options.IncludeDirectories.Contains(directory.Name, StringComparer.OrdinalIgnoreCase)))
								.SelectMany(d => d.GetFeedItems());

							SyndicationFeed feed = new SyndicationFeed(feedItems);
							using (var feedWriter = new XmlTextWriter(Environment.ExpandEnvironmentVariables(options.OutputFileName), Encoding.UTF8))
							{
								feed.SaveAsRss20(feedWriter);
							}

							break;
						}
					case CommandLineOptions.Commands.archive:
						{
							var directories = options.IncludeDirectories.Count == 0 
								? feeder.Directories
								: feeder.Directories.Where(directory => options.IncludeDirectories.Contains(directory.Name, StringComparer.OrdinalIgnoreCase));

							foreach (var directory in directories)
								directory.ArchivateAll();

							break;
						}
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}
