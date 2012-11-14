using System;
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
			/*
def parse_instapaper_date(self, date):
if date == u'today':
    t = time.localtime()
    return u'%d/%d' % (t.tm_mon, t.tm_mday)
return date
*/

			var options = new CommandLineOptions();
			var parser = new CommandLineParser(new CommandLineParserSettings(
				false /*case sensitive*/,
				false /*mutually exclusive*/,
				true /* ignore unknown arguments */,
				Console.Error));

			if (!parser.ParseArguments(args, options))
				Environment.Exit(1);

			Instapaper feeder = new Instapaper
			{
				Username = options.Username,
				Password = options.Password
			};
			feeder.OnMessage += (_, e) => Console.WriteLine(e.Message);

			switch (options.Command)
			{
				case CommandLineOptions.Commands.list:
				{
					feeder.Directories.ForEach(d => Console.WriteLine(d.Name));
					
					break;
				}
				case CommandLineOptions.Commands.rss:
				{
					var feedItems = feeder.Directories
						.Where(directory => options.IncludeDirectories.Contains(directory.Name, StringComparer.OrdinalIgnoreCase))
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
					var directories = feeder.Directories
						.Where(directory => options.IncludeDirectories.Contains(directory.Name, StringComparer.OrdinalIgnoreCase));
					foreach(var directory in directories)
						directory.ArchivateAll();

					break;
				}	
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
