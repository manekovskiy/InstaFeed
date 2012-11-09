using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml.XPath;
using HtmlAgilityPack;

namespace InstaFeed
{
	public sealed class Instapaper
	{
		public sealed class Directory
		{
			// "Older Items" block:
			// <div class="pagination">
			//	<a style="text-decoration: none;" href="/u/1"> « <span style="text-decoration: underline;">Older items</span></a>
			// </div>
			private const string OlderEntriesXpath = ".//*/a[span='Older items']";
			private const string ArticlesXPath = ".//*[@class='tableViewCellTitleLink']";
			
			internal Instapaper Owner { get; set; }
			
			public string Name { get; set; }
			public Uri Link { get; set; }

			public IEnumerable<SyndicationItem> GetFeedItems()
			{
				if (!Owner.IsAuthenticated)
					Owner.Authenticate();

				List<SyndicationItem> items = new List<SyndicationItem>();

				Uri currentPage = new Uri(Link.ToString());
				while (true)
				{
					Owner.Message(new MessageEventArgs(string.Format("Fetching articles list for \"{0}\" folder.", Name)));

					HtmlDocument htmlDocument = Owner.HttpHelper.Get(currentPage);	
					foreach (HtmlNodeNavigator node in htmlDocument.CreateNavigator().Select(ArticlesXPath))
					{
						items.Add(new SyndicationItem(node.CurrentNode.InnerText, string.Empty, new Uri(node.CurrentNode.Attributes["href"].Value)));
						
						Owner.Message(new MessageEventArgs(string.Format("Added article \"{0}\".", node.CurrentNode.InnerText)));
					}

					XPathNodeIterator olderEntriesLink = htmlDocument.CreateNavigator().Select(OlderEntriesXpath);

					if (olderEntriesLink.Count == 0 || !olderEntriesLink.MoveNext())
						break;
					
					currentPage = Url(((HtmlNodeNavigator)olderEntriesLink.Current).CurrentNode.Attributes["href"].Value);
				}

				Owner.Message(new MessageEventArgs(string.Format("Finished fetching articles. Total count: {0}.", items.Count)));
				return items;
			}

			public void ArchivateAll()
			{
				Owner.Message(new MessageEventArgs(string.Format("Archiving all articles in \"{0}\" folder.", Name)));

				HtmlDocument htmlDocument = Owner.HttpHelper.Get(Link);
				
				// Instead of XPath we forced to dance around with LINQ because of HAP, see http://htmlagilitypack.codeplex.com/workitem/33176
				var formKey = htmlDocument.DocumentNode
					.Descendants("input")
					.FirstOrDefault(i => i.Attributes["id"] != null && i.Attributes["id"].Value == "form_key");

				if(formKey == null)
				{
					Owner.Message(new MessageEventArgs("Folder is empty."));
					return;
				}

				string formAction = htmlDocument.DocumentNode
					.Descendants("form")
					.First(f => f.Attributes["action"] != null && f.Attributes["action"].Value.StartsWith("/bulk-archive/"))
					.Attributes["action"].Value;

				var archiveAllFolderUri = Url(formAction);

				Owner.HttpHelper.Post(archiveAllFolderUri, new NameValueCollection { {"form_key", formKey.Attributes["value"].Value} });
				Owner.Message(new MessageEventArgs(string.Format("Articles successfully archived.")));
			}
		}

		private const string DirectoriesXPath = ".//*[@id='folders']/div/a[starts-with(@href, '/u/')]";

		private HttpHelper HttpHelper { get; set; }
		private readonly CookieContainer cookies;

		private bool IsAuthenticated
		{
			get { return cookies.Count > 0; }
		}

		public event EventHandler<MessageEventArgs> OnMessage;

		public string Username { get; set; }
		public string Password { get; set; }

		private List<Directory> directories;
		public List<Directory> Directories
		{
			get
			{
				return directories ?? (directories = GetInstapaperDirectories());
			}
		}

		public Instapaper()
		{
			cookies = new CookieContainer();
			HttpHelper = new HttpHelper(cookies);
		}

		private List<Directory> GetInstapaperDirectories()
		{
			List<Directory> result = new List<Directory>();
			
			if (!IsAuthenticated)
				Authenticate();

			// Read Later directory cannot be removed and always present
			Directory readLaterDirectory = new Directory
			{
				Owner = this,
				Name = "Read Later",
				Link = Url("/u")
			};
			result.Add(readLaterDirectory);

			HtmlDocument frontPageDocument = HttpHelper.Get(readLaterDirectory.Link);
			foreach (HtmlNodeNavigator node in frontPageDocument.CreateNavigator().Select(DirectoriesXPath))
			{
				result.Add(new Directory
				{
					Owner = this,
					Name = node.CurrentNode.InnerText,
					Link = Url(node.CurrentNode.Attributes["href"].Value)
				});
			}

			return result;
		}

		private static readonly Uri LoginPage = Url("/user/login");
		private void Authenticate()
		{
			Contract.Assert(!string.IsNullOrEmpty(Username));
			Contract.Assert(!string.IsNullOrEmpty(Password));

			Message(new MessageEventArgs(string.Format("Authenticating.")));

			NameValueCollection authParams = new NameValueCollection
			{
				{"username", Username},
				{"password", Password}
			};

			var response = HttpHelper.Post(LoginPage, authParams);
			if(!IsAuthenticated)
				throw new Exception("Authentication failed. Response StatusCode: " + ((HttpWebResponse)response).StatusCode);

			Message(new MessageEventArgs(string.Format("Authenticated successfully.")));
		}

		private void Message(MessageEventArgs messageEventArgs)
		{
			if (OnMessage != null)
				OnMessage(this, messageEventArgs);
		}

		private static Uri Url(string relativePath)
		{
			Contract.Assert(relativePath.StartsWith("/"));

			return new Uri(string.Format("http://www.instapaper.com{0}", relativePath));
		}
	}
}