using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Azpe.Viewer
{
	public enum MediaTypes : byte { Image, Video, VideoThumb }
	public enum Statuses : byte { Download, Complete, Error }

	public class MediaInfo
	{
		private AzpViewer	m_parent;
		private int			m_index;
		private int			m_retry = 3;

		public MediaTypes	MediaType	{ get; private set; }
		public string		OrigUrl		{ get; private set; }
		public string		Url			{ get; private set; }
		public string		CachePath	{ get; private set; }
		public Image		Image		{ get; private set; }
		public long			Down		{ get; private set; }
		public long			Total		{ get; private set; }
		public Statuses		Status		{ get; private set; }


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

		private MediaInfo()
		{
		}

		public void StartDownload()
		{
			this.Status = Statuses.Download;

			try
			{
				File.Delete(this.CachePath);
			}
			catch
			{
			}

			new Task(this.Download).Start();

			if (!this.m_disposed)
				this.m_parent.RefreshItem();
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
			do
			{
				if (this.MediaType != MediaTypes.Video)
					this.DownloadImageDo();

				else if (this.MediaType == MediaTypes.Video)
					this.GetLinkVideo();
				
				if (this.m_retry > 0)
					m_retry--;

			} while (this.m_retry > 0 && this.Status == Statuses.Error);
						
			this.Refresh();
		}

		public void Refresh()
		{
			 if (!this.m_disposed && this.m_parent.CurrentIndex == this.m_index)
				 this.m_parent.RefreshItem();
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

					string temp = cachePath + ".tmp";

					try
					{
						using (file = new FileStream(temp, FileMode.OpenOrCreate, FileAccess.Write))
						{
							file.SetLength(0);
							var req = WebRequest.Create(this.Url) as HttpWebRequest;
							req.UserAgent = Program.UserAgent;
							req.Timeout = 5000;
							req.ReadWriteTimeout = 5000;

							using (var res = req.GetResponse())
							{
								this.Total = res.ContentLength;

								if (!this.m_disposed)
									this.m_parent.RefreshItem();

								using (Stream stm = res.GetResponseStream())
								{
									int		read;
									byte[]	buff = new byte[4096];

									while ((read = stm.Read(buff, 0, 4096)) > 0)
									{
										if (this.m_disposed)
											throw new Exception();

										file.Write(buff, 0, read);
										this.Down += read;

										this.Refresh();
									}
								}
							}
						}
					}
					catch (Exception)
					{
						this.Status = Statuses.Error;
						
						throw;
					}

					File.Move(temp, cachePath);

					using (file = new FileStream(cachePath, FileMode.Open, FileAccess.Read))
						this.Image = Image.FromStream(file);
				}
				
				this.Status = Statuses.Complete;
			}
			catch
			{
				this.Status = Statuses.Error;
			}
		}
		
		private static Regex regVineMp4 = new Regex("<video src=\"([^\"])\">", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private void GetLinkVideo()
		{
			if (this.OrigUrl.Contains("vine.co/v/"))
			{
				string body;

				try
				{
					var req = HttpWebRequest.Create(this.OrigUrl) as HttpWebRequest;
					req.UserAgent = Program.UserAgent;

					using (var res = req.GetResponse())
					using (var reader = new StreamReader(res.GetResponseStream()))
						body = reader.ReadToEnd();
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
		}
	}
}
