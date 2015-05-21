using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComputerBeacon.Json;

namespace AZPreviewE
{
	public partial class frmMain : Form
	{
		private const string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0";

		private class Media
		{
			public enum Types : byte { Image, Video, Youtube }

			public Types	MediaType;
			public string	OrigUrl;
			public string	Url;
			public string	CachePath;
			public Image	Image;
		}

		private List<Media>	m_medias	= new List<Media>();
		private int			m_index		= 0;
		private int			m_befIndex	= 0;
		private CustomWnd	m_customWnd;

		public frmMain()
		{
			InitializeComponent();

			this.elem.Dock = DockStyle.Fill;
			this.image.Dock = DockStyle.Fill;

			this.m_customWnd = new CustomWnd(Program.lpClassName, CopyDataProc);

			this.Text = string.Format("[ 0 / 0 ] {0}", Program.ProgramName);
		}

		private void UpdateCheck()
		{
			try
			{
				JsonObject jo;

				var req = HttpWebRequest.Create("https://api.github.com/repos/RyuaNerin/AZPreview-E/releases/latest") as HttpWebRequest;
				req.UserAgent = frmMain.UserAgent;
				using (var res = req.GetResponse())
				using (var net = res.GetResponseStream())
				using (var red = new StreamReader(net))
					jo = new JsonObject(red.ReadToEnd());

				if ((string)jo["tag_name"] != Program.TagName)
				{
					this.Invoke(
						new Action(
							() =>
							{
								try
								{
									if (MessageBox.Show(
											this,
											"새 버전이 업데이트 되었어요!",
											Program.ProgramName,
											MessageBoxButtons.YesNo,
											MessageBoxIcon.Question
											) == DialogResult.Yes)
									{
										Process.Start((string)jo["html_url"]).Dispose();
										this.Close();
									}
								}
								catch
								{
								}
							}
						)
					);
				}
			}
			catch
			{
			}
		}

		private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.m_customWnd.Dispose();
		}

		private void frmMain_Shown(object sender, EventArgs e)
		{
			this.Show();
			this.ReceiveMessage(Program.Arg);

			new Task(() => UpdateCheck()).Start();
		}

		private IntPtr CopyDataProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WinApi.WM_COPYDATA && wParam == Program.WP)
			{
				try
				{
					var data = (WinApi.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(WinApi.COPYDATASTRUCT));

					var buff = new byte[data.cbData];
					Marshal.Copy(data.lpData, buff, 0, data.cbData);

					this.ReceiveMessage(Encoding.UTF8.GetString(buff));

					handled = true;
				}
				catch
				{
				}
			}

			return IntPtr.Zero;
		}
		
		private void ReceiveMessage(string str)
		{
			if (str == "exit")
				this.Close();

			else if (str == "top")
				this.TopMost = !this.TopMost;

			else if (str == "focus")
				WinApi.FocusWindow(this.Handle);

			else if (str == "left")
				this.ItemMove(false);

			else if (str == "right")
				this.ItemMove(true);

			else if (str.Length > 0)
			{
				WinApi.FocusWindow(this.Handle);

				this.m_index	= 0;
				this.m_befIndex	= -1;

				m_medias.ForEach(e => { if (e.Image != null) e.Image.Dispose(); });
				m_medias.Clear();

				if (str != "empty")
				{
					Media.Types mediaType = Media.Types.Image;
					int index = 0;

					foreach (var origUrl in str.Split(','))
					{
						var url = FixUrl(origUrl, ref mediaType);

						if (url == null) continue;

						Media media = new Media();
						media.OrigUrl	= origUrl;
						media.Url		= url;
						media.MediaType	= mediaType;

						if (media.MediaType != Media.Types.Video)
							new Task(() => DownloadImage(media, index)).Start();

						this.m_medias.Add(media);
						index++;
					}
				}

				this.RefreshItems();

				new Task(() => { Thread.Sleep(1000); GC.Collect(); }).Start();
			}

			this.RefreshItems();
		}

		static Regex regYoutube = new Regex(@"^(?:https?://)?(?:(?:(?:www\.)?youtube\.com/(?:v/)?watch[\?#]?.*v=)|(?:youtu\.be/))([A-Za-z0-9_\-]+).*$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
		private string FixUrl(string url, ref Media.Types mediaType)
		{
			if (url.Contains("tweet_video_thumb"))
			{
				mediaType = Media.Types.Video;
				url = url.Replace("tweet_video_thumb", "tweet_video").Replace(".png", ".mp4");
			}
			else if (url.Contains("pbs.twimg.com"))
			{
				if (!url.EndsWith(":orig"))
					url = url + ":orig";
			}
			else if (url.Contains("p.twipple.jp/"))
				url = url.Replace("p.twipple.jp/", "p.twpl.jp/show/orig/");

			else if (url.Contains("twitrpix.com/"))
				url = url.Replace("twitrpix.com/", "img.twitrpix.com/");

			else if (url.Contains("img.ly/"))
				url = url.Replace("img.ly/", "img.ly/show/full/");

			else if (url.Contains("lockerz.com/s/"))
				url = url.Replace("lockerz.com/s/", "api.plixi.com/api/tpapi.svc/imagefromurl?url=http://plixi.com/p/") + "&size=big";

			else if (url.Contains("pikchur.com/"))
				url = url.Replace("pikchur.com/", "img.pikchur.com/pic_") + "_l.jpg";

			else if (url.Contains("puu.sh/"))
			{ }

			else if (url.Contains("pckles.com"))
			{ }

			else if (url.Contains("twitpic.com"))
				url = url.Replace("twitpic.com", "www.twitpic.com/show/full/");

			else if (url.EndsWith(".png") || url.EndsWith(".jpg") || url.EndsWith(".gif"))
			{ }

			else
			{
				Match m = regYoutube.Match(url);
				if (m.Success)
				{
					mediaType = Media.Types.Youtube;
					url = string.Format("http://img.youtube.com/vi/{0}/sddefault.jpg", m.Groups[1].Value);
				}
				else
					return null;
			}

			return url;
		}

		static char[] InvalidChars = Path.GetInvalidFileNameChars();
		private bool GetCache(string url, out string path)
		{
			StringBuilder sb = new StringBuilder(url.Length);
			int  i = 0;
			char c;
			while (i < url.Length)
			{
				c = url[i++];

				if (!InvalidChars.Contains(c))
					sb.Append(c);
				else
					sb.Append('_');
			}

			path = Path.Combine(Program.CacheDir, sb.ToString());

			return File.Exists(path);
		}

		private void DownloadImage(Media media, int index)
		{
			bool exists = GetCache(media.Url, out media.CachePath);

			if (exists)
			{
				File.SetLastAccessTime(media.CachePath, DateTime.Now);

				using (Stream file = new FileStream(media.CachePath, FileMode.Open, FileAccess.Read))
					media.Image = Image.FromStream(file);
			}
			else
			{
				if (!Directory.Exists(Program.CacheDir))
					Directory.CreateDirectory(Program.CacheDir);

				using (Stream file = new FileStream(media.CachePath, FileMode.CreateNew, FileAccess.ReadWrite))
				{
					var req = WebRequest.Create(media.Url) as HttpWebRequest;
					req.UserAgent = frmMain.UserAgent;
					using (var res = req.GetResponse())
					{
						using (Stream stm = res.GetResponseStream())
						{
							int		read;
							byte[]	buff = new byte[4096];

							while ((read = stm.Read(buff, 0, 4096)) > 0)
								file.Write(buff, 0, read);
						}
					}

					file.Position = 0;
					media.Image = Image.FromStream(file);
				}
			}
			
			this.RefreshItems();
		}

		private void RefreshItems()
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new Action(RefreshItems));
			}
			else
			{
				if (this.m_medias.Count == 0)
				{
					this.Text = string.Format("[ 0 / 0 ] {0}", Program.ProgramName);

					this.image.Visible = true;
					this.elem.Visible = false;
					this.media.Media.Stop();

					this.image.SetImage(null);
				}
				else
				{
					this.Text = string.Format("[ {0} / {1} ] {2}", this.m_index + 1, this.m_medias.Count, Program.ProgramName);

					this.m_befIndex = this.m_index;
					Media media = this.m_medias[this.m_index];
					switch (media.MediaType)
					{
						case Media.Types.Image:
						case Media.Types.Youtube:
							this.image.Visible = true;
							this.elem.Visible = false;
							this.media.Media.Stop();

							this.image.SetImage(this.m_medias[this.m_index].Image);
							break;

						case Media.Types.Video:
							this.image.Visible = false;
							this.elem.Visible = true;

							this.media.Media.Source = null;
							this.media.Media.Source = new Uri(this.m_medias[this.m_index].Url);
							break;
					}
				}
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Left:		this.ItemMove(false);	return true;
				case Keys.Right:	this.ItemMove(true);	return true;

				//////////////////////////////////////////////////////////////////////////

				case Keys.Control | Keys.Left:	this.image.ImageMove(-1, 0);	return true;
				case Keys.Control | Keys.Right:	this.image.ImageMove(1, 0);		return true;
				case Keys.Control | Keys.Up:	this.image.ImageMove(0, -1);	return true;
				case Keys.Control | Keys.Down:	this.image.ImageMove(0, 1);		return true;

				//////////////////////////////////////////////////////////////////////////

				case Keys.Shift | Keys.Left:
					{
						int val = this.Left - 20;
						if (val < Screen.PrimaryScreen.WorkingArea.X - this.Width)
							val = Screen.PrimaryScreen.WorkingArea.X - this.Width;
						this.Left = val;
					}
					break;

				case Keys.Shift | Keys.Right:
					{
						int val = this.Left + 20;
						if (val > Screen.PrimaryScreen.WorkingArea.Width)
							val = Screen.PrimaryScreen.WorkingArea.Width;
						this.Left = val;
					}
					break;

				case Keys.Shift | Keys.Up:
					{
						int val = this.Top - 20;
						if (val < Screen.PrimaryScreen.WorkingArea.Y - this.Height)
							val = Screen.PrimaryScreen.WorkingArea.Y - this.Height;
						this.Top = val;
					}
					break;

				case Keys.Shift | Keys.Down:
					{
						int val  = this.Top + 20;
						if (val > Screen.PrimaryScreen.WorkingArea.Height)
							val = Screen.PrimaryScreen.WorkingArea.Height;
						this.Top = val;
					}
					break;

				//////////////////////////////////////////////////////////////////////////

				case Keys.Alt | Keys.Left:
					{
						int val = this.Width - 20;
						if (val < this.MinimumSize.Width)
							val = this.MinimumSize.Width;
						this.Width = val;
					}
					break;

				case Keys.Alt | Keys.Right:
					{
						int val = this.Width + 20;
						if (this.Width > Screen.PrimaryScreen.WorkingArea.Width - this.Left)
							this.Width = Screen.PrimaryScreen.WorkingArea.Width - this.Left;
						this.Width = val;
					}
					break;

				case Keys.Alt | Keys.Up:
					{
						int val = this.Height - 20;
						if (val < this.MinimumSize.Height)
							val = this.MinimumSize.Height;
						this.Height = val;
					}
					break;

				case Keys.Alt | Keys.Down:
					{
						int val = this.Height + 20;
						if (val > Screen.PrimaryScreen.WorkingArea.Height - this.Top)
							val = Screen.PrimaryScreen.WorkingArea.Height - this.Top;
						this.Height = val;
					}
					break;

				//////////////////////////////////////////////////////////////////////////

				case Keys.Control | Keys.S:
					{
						Media media = this.m_medias[this.m_index];

						if (media.MediaType == Media.Types.Image)
						{
							this.sfd.FileName = Path.Combine(this.sfd.InitialDirectory, Path.GetFileName(new Uri(media.Url.Replace(":orig", "")).AbsolutePath));

							string ext = Path.GetExtension(this.sfd.FileName);
							this.sfd.Filter = string.Format("*{0}|*{0}", ext);

							if (this.sfd.ShowDialog() == DialogResult.OK)
								File.Copy(media.CachePath, this.sfd.FileName);
						}
					}
					return true;

				case Keys.G:
				case Keys.A:
				case Keys.Tab:
					WinApi.FocusWindow(WinApi.FindWindow("Azurea_TwitterClient", null));
					return true;


				case Keys.W:
					this.GoBrowser();
					return true;


				case Keys.Z:
					this.image.ToggleZoomMode();
					this.media.Media.Stretch =
						this.media.Media.Stretch == System.Windows.Media.Stretch.Uniform ?
							System.Windows.Media.Stretch.None :
							System.Windows.Media.Stretch.Uniform;
					return true;


				case Keys.F1:
					Process.Start("http://ryuanerin.github.io/AZPreview-E").Dispose();
					return true;

				case Keys.Escape:
				case Keys.Enter:
					this.Close();
					return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void ItemMove(bool moveNext)
		{
			if (moveNext)
			{
				if (this.m_index < this.m_medias.Count - 1)
				{
					this.m_index++;
					this.RefreshItems();
				}
			}
			else
			{
				if (0 < this.m_index)
				{
					this.m_index--;
					this.RefreshItems();
				}
			}
		}

		private void GoBrowser()
		{
			Media media = this.m_medias[this.m_index];

			try
			{
				if (media.MediaType == Media.Types.Youtube)
					Process.Start(media.OrigUrl).Dispose();
				else
					Process.Start(media.Url).Dispose();
			}
			catch
			{
			}
		}

		private void frmMain_ResizeEnd(object sender, EventArgs e)
		{
			Settings.Save();
		}
	}
}
