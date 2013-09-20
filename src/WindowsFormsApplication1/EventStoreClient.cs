namespace WindowsFormsApplication1
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.ServiceModel.Syndication;
	using System.Xml;

	public class EventStoreClient
	{
		public static SyndicationLink GetNamedLink(IEnumerable<SyndicationLink> links, string name)
		{
			return links.FirstOrDefault(link => link.RelationshipType == name);
		}

		public static Uri GetLast(Uri head)
		{
			var request = (HttpWebRequest) WebRequest.Create(head);
			request.Credentials = new NetworkCredential("admin", "changeit");
			request.Accept = "application/atom+xml";
			try
			{
				using (var response = (HttpWebResponse) request.GetResponse())
				{
					if (response.StatusCode == HttpStatusCode.NotFound)
					{
						return null;
					}
					using (var xmlreader = XmlReader.Create(response.GetResponseStream()))
					{
						var feed = SyndicationFeed.Load(xmlreader);
						var last = GetNamedLink(feed.Links, "last");
						return (last != null) ? last.Uri : GetNamedLink(feed.Links, "self").Uri;
					}
				}
			}
			catch (WebException ex)
			{
				if (((HttpWebResponse) ex.Response).StatusCode == HttpStatusCode.NotFound)
				{
					return null;
				}
				throw;
			}
		}

		public static void ProcessItem(SyndicationItem item, Action<string> read)
		{
			Console.WriteLine(item.Title.Text);
			//get events
			var request = (HttpWebRequest) WebRequest.Create(GetNamedLink(item.Links, "alternate").Uri);
			request.Credentials = new NetworkCredential("admin", "changeit");
			request.Accept = "application/json";
			using (var response = request.GetResponse())
			{
				var streamReader = new StreamReader(response.GetResponseStream());

				var messaqge = streamReader.ReadToEnd();
				read(messaqge);
			}
		}

		public static Uri ReadPrevious(Uri uri, Action<string> read)
		{
			var request = (HttpWebRequest) WebRequest.Create(uri);
			request.Credentials = new NetworkCredential("admin", "changeit");
			request.Accept = "application/atom+xml";
			using (var response = request.GetResponse())
			{
				using (var xmlreader = XmlReader.Create(response.GetResponseStream()))
				{
					var feed = SyndicationFeed.Load(xmlreader);
					foreach (var item in feed.Items.Reverse())
					{
						ProcessItem(item, read);
					}
					var prev = GetNamedLink(feed.Links, "previous");
					return prev == null ? uri : prev.Uri;
				}
			}
		}

		public static void PostMessage(string user, string text)
		{
			var message = @"[{'eventType':'chatMessage', 'eventId' :
				'" + Guid.NewGuid() + @"',
				'data' :
				{
					'user': '" + user + @"',
					'text':  '" + text + @"' 
					}}]";
			var request = WebRequest.Create("http://ha.geteventstore.com:2113/streams/chat-foo");
			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = message.Length;
			using (var sw = new StreamWriter(request.GetRequestStream()))
			{
				sw.Write(message);
			}
			using (var response = request.GetResponse())
			{
				response.Close();
			}
		}
	}
}