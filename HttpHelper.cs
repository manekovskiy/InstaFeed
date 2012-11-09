using System;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace InstaFeed
{
	internal class HttpHelper
	{
		private readonly CookieContainer cookies;
		public HttpHelper(CookieContainer cookies)
		{
			this.cookies = cookies;
		}

		public WebResponse Post(Uri link, NameValueCollection parameters)
		{
			var request = CreateHttpRequest(link, "POST");

			// produces string in format param1=value1&param2=value2&param3=value3&...&paramN=valueN
			string paramString = string.Join("&", parameters.AllKeys.Select(key => string.Format("{0}={1}", key, parameters[(string) key])));
			byte[] paramBytes = Encoding.UTF8.GetBytes(paramString);
			using (Stream requestStream = request.GetRequestStream())
			{
				requestStream.Write(paramBytes, 0, paramBytes.Length);

				var response = request.GetResponse();
				using (StreamReader responseStream = new StreamReader(response.GetResponseStream()))
				{
					responseStream.ReadToEnd();
				}

				return response;
			}
		}

		public HtmlDocument Get(Uri link)
		{
			HtmlDocument document = new HtmlDocument();
			document.Load(
				CreateHttpRequest(link, "GET")
					.GetResponse()
					.GetResponseStream());

			return document;
		}

		private HttpWebRequest CreateHttpRequest(Uri link, string method)
		{
			Contract.Assert(method == "GET" || method == "POST");

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
			request.CookieContainer = cookies;
			request.UserAgent = "Mozilla/5.0(compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
			request.Method = method;
			request.ContentType = "application/x-www-form-urlencoded";

			return request;
		}
	}
}