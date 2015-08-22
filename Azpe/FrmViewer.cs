using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Azpe.Viewer;

namespace Azpe
{
	public partial class FrmViewer : Form
	{
		public static FrmViewer Create(long tweetId, string urls)
		{
			var lstInfo	= new List<MediaInfo>();
			var lstUrl	= new List<string>();

			Uri uri;
			int index = 0;
			
			string[] ss = urls.Split(',');

			for (int i = 2; i < ss.Length; ++i)
			{
				string url = ss[i];
				if (Uri.TryCreate(url, UriKind.Absolute, out uri))
				{
					if (!lstUrl.Contains(url.Replace("https", "http")))
					{
						lstUrl.Add(url.Replace("https", "http"));
						lstInfo.Add(new MediaInfo(url, index++));
					}
				}
			}

			return lstInfo.Count > 0 ? new FrmViewer(tweetId, lstInfo) : null;
		}

		private List<MediaInfo> m_lst;
		private int				m_index = 0;
		private int				m_indexBef = -1;
		private bool			m_containsVideo;

		private ImageViewer		m_pic;
		private ElementHost		m_host;
		private VideoViewer		m_media;

		private FrmViewer(long tweetId, List<MediaInfo> lst)
		{
			this.m_lst = lst;
			this.m_containsVideo = lst.Exists(e => e.MediaType == MediaTypes.Video);
			this.TweetId = tweetId;

			InitializeComponent();
			this.Text = "[ 0 / 0 ] Azpe";
						
			this.SuspendLayout();

			this.m_pic			= new ImageViewer();
			this.m_pic.Dock		= DockStyle.Fill;
			this.m_pic.Visible	= false;
			this.Controls.Add(this.m_pic);

			if (this.m_containsVideo)
			{
				this.m_media		= new VideoViewer();
				this.m_host			= new ElementHost();
				this.m_host.Visible	= false;
				this.m_host.Dock	= DockStyle.Fill;
				this.m_host.Child	= this.m_media;
				this.Controls.Add(this.m_host);
			}

			this.ResumeLayout(false);

			lst.ForEach(e => { e.SetParent(this); e.StartDownload(); });

			if (Settings.Left != -1)	this.Left	= Settings.Left;
			if (Settings.Top != -1)		this.Top	= Settings.Top;
			if (Settings.Width != -1)	this.Width	= Settings.Width;
			if (Settings.Height != -1)	this.Height	= Settings.Height;
		}

		public new void Activate()
		{
			if (this.WindowState == FormWindowState.Minimized)
				this.WindowState = FormWindowState.Normal;
			base.Activate();
			NativeMethods.SetForegroundWindow(this.Handle);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.sfd != null)
				{
					this.sfd.Dispose();
					this.sfd = null;
				}

				if (this.m_pic != null)
				{
					this.m_pic.Dispose();
					this.m_pic = null;
				}

				if (this.m_host != null)
				{
					this.m_host.Dispose();
					this.m_host	= null;
				}

				this.m_lst.ForEach(e => e.Dispose());
			}
			base.Dispose(disposing);
		}

		public int MediaCount
		{
			get
			{
				return this.m_lst.Count;
			}
		}

		public MediaInfo Current
		{
			get
			{
				return this.m_lst.Count == 0 ? null :  this.m_lst[this.m_index];
			}
		}

		public int CurrentIndex
		{
			get
			{
				return this.m_index;
			}
			set
			{
				if (0 <= value && value < this.m_lst.Count)
					this.m_index = value;

				this.RefreshItem();
			}
		}

		public long TweetId { get; private set; }

		private void FrmViewer_Activated(object sender, EventArgs e)
		{
			this.BackColor			= Color.White;
			this.m_pic.BackColor	= Color.White;
			if (this.m_containsVideo)
				this.m_media.Background = System.Windows.Media.Brushes.White;
			this.RefreshItem();
		}

		private void FrmViewer_Deactivate(object sender, EventArgs e)
		{
			try
			{
				this.BackColor			= Color.WhiteSmoke;
				this.m_pic.BackColor	= Color.WhiteSmoke;
				if (this.m_containsVideo)
					this.m_media.Background = System.Windows.Media.Brushes.WhiteSmoke;
				this.RefreshItem();
			}
			catch
			{
			}
		}


		public void StartDownload()
		{
			var cur = this.Current;
			if (cur.Status == Statuses.Error)
				cur.StartDownload();
		}


		public void ToggleZoomMode()
		{
			this.m_pic.ToggleZoomMode();
			if (this.m_containsVideo)
				this.m_media.Media.Stretch =
							this.m_media.Media.Stretch == System.Windows.Media.Stretch.Uniform ?
								System.Windows.Media.Stretch.None :
								System.Windows.Media.Stretch.Uniform;
		}

		public void ImageMove(int x, int y)
		{
			if (this.Current.MediaType != MediaTypes.Video)
				this.m_pic.ImageMove(x, y);
		}

		public void RefreshItem()
		{
			if (this.InvokeRequired)
			{
				try
				{
					this.Invoke(new Action(this.RefreshItem));
				}
				catch
				{
				}
			}
			else
			{
				if (this.m_lst.Count == 0)
				{
					this.Text = "[ 0 / 0 ] Azpe";

					this.m_pic.Visible	= false;
					this.m_host.Visible = false;
				}
				else
				{
					this.Text = string.Format("[ {0} / {1} ] Azpe", this.m_index + 1, this.m_lst.Count);

					var cur = this.Current;
					switch (cur.MediaType)
					{
						case MediaTypes.Image:
						case MediaTypes.VideoThumb:
						case MediaTypes.PageThumb:
							this.m_pic.Visible	= true;
							if (this.m_containsVideo)
							{
								this.m_host.Visible = false;
								this.m_media.Pause();
							}

							switch (cur.Status)
							{
								case Statuses.Download:
									this.m_pic.SetDownload(cur.Progress, cur.Speed);
									break;

								case Statuses.Complete:
									this.m_pic.SetImage(cur.Image, cur.MediaType);
									break;

								case Statuses.Error:
									this.m_pic.SetError();
									break;
							}

							break;

						case MediaTypes.Video:
							switch (cur.Status)
							{
								case Statuses.Download:
									this.m_pic.Visible	= true;
									this.m_host.Visible = false;
									this.m_pic.SetDownload(0, 0);
									break;

								case Statuses.Complete:
									this.m_pic.Visible	= false;
									this.m_host.Visible = true;

									if (this.m_indexBef != this.m_index)
									{
										this.m_media.Pause();
										this.m_indexBef	 = this.m_index;

										this.m_media.Media.Source = null;
										this.m_media.Media.Source = new Uri(cur.Url);
									}

									break;

								case Statuses.Error:
									this.m_pic.Visible	= true;
									this.m_host.Visible = false;
									this.m_pic.SetError();
									break;
							}

							break;
					}
				}
			}
		}

		#region Key Event
		private void CalcScreen()
		{
			this.m_xMin = this.m_yMin = int.MaxValue;
			this.m_xMax = this.m_yMax = int.MinValue;

			foreach (Screen screen in Screen.AllScreens)
			{
				var bounds = screen.Bounds;

				this.m_xMin = Math.Min(this.m_xMin, bounds.Left);
				this.m_yMin = Math.Min(this.m_yMin, bounds.Y);

				this.m_xMax = Math.Max(this.m_xMax, bounds.Right);
				this.m_yMax = Math.Max(this.m_yMax, bounds.Bottom);
			}
		}

		private const int WM_KEYDOWN = 0x100;
		private const int WM_KEYUP = 0x101;
		private const int WM_SYSKEYDOWN = 0x104;

		private int		m_move;
		private int		m_xMin, m_xMax;
		private int		m_yMin, m_yMax;
		private bool	m_keyRepeat;

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Left:		this.CurrentIndex--;	return true;
				case Keys.Right:	this.CurrentIndex++;	return true;

				//////////////////////////////////////////////////////////////////////////

				case Keys.Control | Keys.Left:	this.ImageMove(-1, 0);	return true;
				case Keys.Control | Keys.Right:	this.ImageMove(1, 0);	return true;

				case Keys.Control | Keys.Up:
					if (this.Current.MediaType != MediaTypes.Video)
						this.ImageMove(0, -1);
					else
						this.m_media.Media.Volume = (this.m_media.Media.Volume + 0.1) % 1;
					return true;

				case Keys.Control | Keys.Down:
					if (this.Current.MediaType != MediaTypes.Video)
						this.ImageMove(0, 1);
					else
					{
						var v = this.m_media.Media.Volume + 0.1;
						if (v < 0) v = 0;
						this.m_media.Media.Volume = v;
					}
					return true;

				//////////////////////////////////////////////////////////////////////////

				case Keys.Shift | Keys.Left:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						if (!this.m_keyRepeat)
						{
							this.m_move = 0;
							this.m_keyRepeat = true;
							this.CalcScreen();
						}

						if (this.m_move < 30) this.m_move++;

						var val = this.Left - this.m_move;
						if (val < this.m_xMin - this.Width + 20)
							val = this.m_xMin - this.Width + 20;
						this.Left = val;
					}
					else
					{
						this.m_keyRepeat = false;
					}
					break;

				case Keys.Shift | Keys.Right:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						if (!this.m_keyRepeat)
						{
							this.m_move = 0;
							this.m_keyRepeat = true;
							this.CalcScreen();
						}

						if (this.m_move < 30) this.m_move++;

						var val = this.Left + this.m_move;
						if (val > this.m_xMax - 20)
							val = this.m_xMax - 20;
						this.Left = val;
					}
					else
					{
						this.m_keyRepeat = false;
					}
					break;

				case Keys.Shift | Keys.Up:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						if (!this.m_keyRepeat)
						{
							this.m_move = 0;
							this.m_keyRepeat = true;
							this.CalcScreen();
						}

						if (this.m_move < 30) this.m_move++;

						var val = this.Top - this.m_move;
						if (val < this.m_yMin - this.Height + 20)
							val = this.m_yMin - this.Height + 20;
						this.Top = val;
					}
					else
					{
						this.m_keyRepeat = false;
					}
					break;

				case Keys.Shift | Keys.Down:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						if (!this.m_keyRepeat)
						{
							this.m_move = 0;
							this.m_keyRepeat = true;
							this.CalcScreen();
						}

						if (this.m_move < 30) this.m_move++;

						var val  = this.Top + this.m_move;
						if (val > this.m_yMax)
							val = this.m_yMax;
						this.Top = val;
					}
					else
					{
						this.m_keyRepeat = false;
					}
					break;

				//////////////////////////////////////////////////////////////////////////

				case Keys.Alt | Keys.Left:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						int val = this.Width - 20;
						if (val < this.MinimumSize.Width)
							val = this.MinimumSize.Width;
						this.Width = val;
					}
					break;

				case Keys.Alt | Keys.Right:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						int val = this.Width + 20;
						var scr = Screen.FromHandle(this.Handle);
						if (this.Width > scr.Bounds.Width - this.Left)
							this.Width = scr.Bounds.Width - this.Left;
						this.Width = val;
					}
					break;

				case Keys.Alt | Keys.Up:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						int val = this.Height - 20;
						if (val < this.MinimumSize.Height)
							val = this.MinimumSize.Height;
						this.Height = val;
					}
					break;

				case Keys.Alt | Keys.Down:
					if (msg.Msg == WM_KEYDOWN || msg.Msg == WM_SYSKEYDOWN)
					{
						int val = this.Height + 20;
						var scr = Screen.FromHandle(this.Handle);
						if (val > scr.Bounds.Height - this.Top)
							val = scr.Bounds.Height - this.Top;
						this.Height = val;
					}
					break;

				//////////////////////////////////////////////////////////////////////////

				case Keys.Control | Keys.S:
					{
						var cur = this.Current;

						if (cur.MediaType == MediaTypes.Image)
						{
							string filename = Path.GetFileName(new Uri(cur.Url.Replace(":orig", "")).AbsolutePath);

							if (!string.IsNullOrEmpty(Settings.SavePath) && Directory.Exists(Settings.SavePath))
								this.sfd.FileName = Path.Combine(Settings.SavePath, filename);
							else
								this.sfd.FileName = Path.Combine(this.sfd.InitialDirectory, filename);

							string ext = Path.GetExtension(this.sfd.FileName);
							this.sfd.Filter = string.Format("*{0}|*{0}", ext);

							if (this.sfd.ShowDialog() == DialogResult.OK)
							{
								File.Copy(cur.CachePath, this.sfd.FileName);

								Settings.SavePath = Path.GetFileName(this.sfd.FileName);
								Settings.Save();
							}
						}
					}
					return true;
					
				case Keys.Control | Keys.R:
					this.StartDownload();
					return true;
					
				case Keys.G:
				case Keys.Tab:
					Handler.ActivateAzurea();
					return true;

				case Keys.Shift | Keys.Tab:
					Handler.NextWindow(this).Activate();
					return true;

				case Keys.W:
					Process.Start(this.Current.OrigUrl);
					return true;
					
				case Keys.Z:
					this.ToggleZoomMode();
					return true;

				case Keys.V:
					MessageBox.Show(this, Program.TagName, "Azpe", MessageBoxButtons.OK);
					return true;
					
				case Keys.F1:
					Process.Start("http://ryuanerin.github.io/Azpe");
					return true;

				case Keys.M:
					if (this.Current.MediaType == MediaTypes.Video)
						this.m_media.Media.IsMuted = !this.m_media.Media.IsMuted;
					return true;

				case Keys.Escape:
				case Keys.Enter:
				case Keys.Control | Keys.W:
					this.Close();
					return true;

				case Keys.Shift | Keys.Escape:
					this.Close();
					Handler.CloseAll();
					return true;

				case Keys.Alt | Keys.Enter:
					Application.Exit();
					return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
		#endregion

		private void FrmViewer_FormClosing(object sender, FormClosingEventArgs e)
		{
			Settings.Left	= this.Left;
			Settings.Top	= this.Top;
			Settings.Width	= this.Width;
			Settings.Height	= this.Height;
			Settings.Save();
		}
	}
}
