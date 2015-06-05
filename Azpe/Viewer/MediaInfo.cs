using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Azpe.Viewer
{
	public enum MediaTypes	: byte { Image,		Video,		VideoThumb	}
	public enum Statuses	: byte { Download,	Complete,	Error		}

	public class MediaInfo : IDisposable
	{
		private AzpViewer	m_parent;
		private int			m_index;

		private WebClient	m_web;
		private string		m_temp;
		private float		m_speed2;
		private long		m_down2;
		private DateTime	m_date;

		public MediaTypes	MediaType	{ get; private set; }
		public string		OrigUrl		{ get; private set; }
		public string		Url			{ get; private set; }
		public string		CachePath	{ get; private set; }
		public Image		Image		{ get; private set; }
		public float		Progress	{ get; private set; }
		public float		Speed		{ get; private set; }
		public Statuses		Status		{ get; private set; }

		private MediaInfo()
		{
			this.m_web = new WebClient();
			this.m_web.DownloadFileCompleted += DownloadFileCompleted;
			this.m_web.DownloadProgressChanged += DownloadProgressChanged;
		}

		private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			DateTime dt = DateTime.UtcNow;
			TimeSpan ts = dt - this.m_date;

			if (this.m_down2 == 0)
			{
				this.Speed		= e.BytesReceived / (float)ts.TotalSeconds;

				this.m_date		= dt;
				this.m_down2	= e.BytesReceived;
			}
			else if (ts.TotalMilliseconds > 250)
			{
				this.m_speed2	= (this.m_down2 - e.BytesReceived) / (float)ts.TotalSeconds;
				this.Speed		= (this.Speed + this.m_down2) / 2;

				this.m_date		= dt;
				this.m_down2	= e.BytesReceived;
			}

			this.Progress = e.TotalBytesToReceive == 0 ? 0 : e.BytesReceived / (float)e.TotalBytesToReceive;

			this.Refresh();
		}

		private void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				File.Delete(this.m_temp);
				Settings.DelCacheFile((string)e.UserState, null);

				this.Status = Statuses.Error;
				this.Refresh();
			}
		}

		~MediaInfo()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private bool m_disposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if (!this.m_disposed)
			{
				this.m_disposed = true;

				if (this.Image != null)
				{
					this.Image.Dispose();
					this.Image = null;
				}

				if (this.m_web != null)
				{
					this.m_web.CancelAsync();
					this.m_web.Dispose();
				}
			}
		}

		public static MediaInfo Create(AzpViewer parent, string url, int index)
		{
			MediaTypes	mediaType;
			string		urlFixed;

			urlFixed = MediaInfo.FixUrl(url, out mediaType);

			if (urlFixed == null)
				return null;

			var media = new MediaInfo();

			media.m_parent	= parent;
			media.m_index	= index;

			media.OrigUrl	= url;
			media.Url		= MediaInfo.FixUrl(url, out mediaType);
			media.MediaType	= mediaType;

			media.StartDownload();

			return media;
		}

		public void Refresh()
		{
			if (!this.m_disposed && this.m_parent.CurrentIndex == this.m_index)
				this.m_parent.Refresh();
		}

		public void StartDownload()
		{
			this.Status = Statuses.Download;

			try
			{
				File.Delete(this.CachePath);
			}
			catch
			{ }

			new Task(this.Download).Start();

			this.Refresh();
		}
		
		private static RegexOptions regRules = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;
		private static Regex regYoutube	= new Regex(@"^(?:https?://)?(?:(?:(?:www\.)?youtube\.com/(?:v/)?watch[\?#]?.*v=)|(?:youtu\.be/))([A-Za-z0-9_\-]+).*$", regRules);
		private static Regex regVine	= new Regex(@"^https?://vine.co/v/[a-zA-Z0-9]+$", regRules);

		private static string FixUrl(string url, out MediaTypes mediaType)
		{
			mediaType = MediaTypes.Image;

			if (url.Contains("tweet_video_thumb"))
			{
				mediaType = MediaTypes.Video;
				return url.Replace("tweet_video_thumb", "tweet_video").Replace(".png", ".mp4");
			}

			if (url.Contains("pbs.twimg.com"))
				return url.EndsWith(":orig") ? url : url + ":orig";

			if (url.Contains("p.twipple.jp/"))
				return url.Replace("p.twipple.jp/", "p.twpl.jp/show/orig/");

			if (url.Contains("twitrpix.com/"))
				return url.Replace("twitrpix.com/", "img.twitrpix.com/");

			if (url.Contains("img.ly/"))
				return url.Replace("img.ly/", "img.ly/show/full/");

			if (url.Contains("lockerz.com/s/"))
				return url.Replace("lockerz.com/s/", "api.plixi.com/api/tpapi.svc/imagefromurl?url=http://plixi.com/p/") + "&size=big";

			if (url.Contains("pikchur.com/"))
				return url.Replace("pikchur.com/", "img.pikchur.com/pic_") + "_l.jpg";

			if (url.Contains("puu.sh/"))
				return url;

			if (url.Contains("pckles.com"))
				return url;

			if (url.Contains("twitpic.com"))
				return url.Replace("twitpic.com", "www.twitpic.com/show/full/");

			if (url.EndsWith(".png") || url.EndsWith(".jpg") || url.EndsWith(".gif"))
				return url;
			
			Match m;
			
			m = regYoutube.Match(url);
			if (m.Success)
			{
				mediaType = MediaTypes.VideoThumb;
				return string.Format("http://img.youtube.com/vi/{0}/sddefault.jpg", m.Groups[1].Value);
			}

			m = regVine.Match(url);
			if (m.Success)
			{
				mediaType = MediaTypes.Video;
				return url;
			}

			return null;
		}

		private static char[] InvalidChars = Path.GetInvalidFileNameChars();
		private static Stream GetCache(string url, out string path)
		{
			path = Path.Combine(Program.CacheDir, Settings.getCacheFileName(url));

			if (File.Exists(path))
			{
				File.SetLastAccessTimeUtc(path, DateTime.UtcNow);
				return new FileStream(path, FileMode.Open, FileAccess.Read);
			}
			else
			{
				return null;
			}
		}

		private void Download()
		{
			int retry = 3;
			do
			{
				if (this.MediaType != MediaTypes.Video)
					this.DownloadImageDo();

				else if (this.MediaType == MediaTypes.Video)
					this.GetLinkVideo();

				else
					this.Status = Statuses.Complete;
				
			} while (--retry > 0 && this.Status == Statuses.Error);
						
			this.Refresh();
		}

		private void DownloadImageDo()
		{
			try
			{
				string cachePath;
				Stream file = MediaInfo.GetCache(this.Url, out cachePath);

				this.CachePath = cachePath;

				if (file != null)
				{
					using (file)
						this.Image = Image.FromStream(file);
				}
				else
				{
					if (!Directory.Exists(Program.CacheDir))
						Directory.CreateDirectory(Program.CacheDir);

					this.m_temp = cachePath + ".tmp";

					this.m_date = DateTime.UtcNow;
					this.m_web.Headers.Add(HttpRequestHeader.UserAgent, Program.UserAgent);
					this.m_web.DownloadFileAsync(new Uri(this.Url), this.m_temp, this.Url);

					while (this.m_web.IsBusy)
						Thread.Sleep(100);

					File.Move(this.m_temp, cachePath);

					using (file = new FileStream(cachePath, FileMode.Open, FileAccess.Read))
						this.Image = Image.FromStream(file);
				}
				
				this.Status = Statuses.Complete;
			}
			catch
			{
				this.Status = Statuses.Error;
			}

			this.Refresh();
		}
		
		private static Regex regVineMp4 = new Regex("<video src=\"([^\"])\">", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private void GetLinkVideo()
		{
			if (this.OrigUrl.Contains("vine.co/v/"))
			{
				string body;

				try
				{
					this.m_web.Headers.Add(HttpRequestHeader.UserAgent, Program.UserAgent);

					body = this.m_web.DownloadString(this.Url);
				}
				catch
				{
					this.Status = Statuses.Error;
					return;
				}

				var m = regVine.Match(body);
				if (m.Success)
				{
					this.Status = Statuses.Complete;
					this.Url = m.Groups[1].Value;
				}
				else
				{
					this.Status = Statuses.Error;
				}
			}

			this.Status = Statuses.Complete;
		}
	}
}
