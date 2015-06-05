using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using ElementHost = System.Windows.Forms.Integration.ElementHost;
using MediaElement = System.Windows.Controls.MediaElement;

namespace Azpe.Viewer
{
	public class AzpViewer
	{
		private List<MediaInfo> m_lst = new List<MediaInfo>();
		private int				m_index;
		private int				m_indexBef;

		private Form			m_form;
		private ImageViwer		m_pic;
		private ElementHost		m_host;
		private VideoViewer		m_media;

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
				if (this.m_lst.Count == 0)
					return null;
				else
					return this.m_lst[this.m_index];
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

				this.Refresh();
			}
		}

		public AzpViewer(Form form)
		{
			this.m_form = form;
			this.m_form.Activated	+= GotFocus;
			this.m_form.Deactivate	+= LostFocus;

			this.m_pic		= new ImageViwer();
			this.m_host		= new ElementHost();
			this.m_media	= new VideoViewer();

			this.m_form.SuspendLayout();

			this.m_pic.Visible	= false;
			this.m_host.Visible	= false;

			this.m_pic.Dock		= DockStyle.Fill;
			this.m_host.Dock	= DockStyle.Fill;

			this.m_host.Child	= this.m_media;

			this.m_pic.MouseClick += m_pic_MouseClick;

			this.m_form.Controls.Add(this.m_pic);
			this.m_form.Controls.Add(this.m_host);

			this.m_form.ResumeLayout();
		}

		private void m_pic_MouseClick(object sender, MouseEventArgs e)
		{
			this.DownloadImage();
		}

		public void DownloadImage()
		{
			var cur = this.Current;
			if (cur.Status == Statuses.Error)
				cur.StartDownload();
		}
		
		private void GotFocus(object sender, EventArgs e)
		{
			this.m_form.BackColor	= Color.White;
			this.m_pic.BackColor	= Color.White;
			this.m_media.Background = System.Windows.Media.Brushes.White;
			this.Refresh();
		}

		private void LostFocus(object sender, EventArgs e)
		{
			this.m_form.BackColor	= Color.WhiteSmoke;
			this.m_pic.BackColor	= Color.WhiteSmoke;
			this.m_media.Background	= System.Windows.Media.Brushes.WhiteSmoke;
			this.Refresh();
		}
		
		public void AddUrl(string urls)
		{
			this.m_indexBef	= -1;
			this.m_index	= 0;

			this.m_lst.ForEach(e => e.Dispose());
			this.m_lst.Clear();

			this.m_pic.SetDownload(0, 0);
			
			this.Refresh();

			if (urls != "init")
			{
				var lstUrls = new List<string>();

				Uri uri;
				int index = 0;
				foreach (var url in urls.Split(','))
				{
					if (Uri.TryCreate(url, UriKind.Absolute, out uri))
					{
						if (!lstUrls.Contains(url))
						{
							lstUrls.Add(url);

							var info = MediaInfo.Create(this, url, index++);
							if (info != null)
								this.m_lst.Add(info);
						}
					}
				}
			}
			
			this.Refresh();
		}

		public void Refresh()
		{
			if (this.m_form.InvokeRequired)
			{
				this.m_form.Invoke(new Action(this.Refresh));
			}
			else
			{
				if (this.m_lst.Count == 0)
				{
					this.m_form.Text = string.Format("[ 0 / 0 ] {0}", Program.ProgramName);

					this.m_pic.Visible	= false;
					this.m_host.Visible = false;
				}
				else
				{
					this.m_form.Text = string.Format("[ {0} / {1} ] {2}", this.m_index + 1, this.m_lst.Count, Program.ProgramName);

					var cur = this.Current;
					switch (cur.MediaType)
					{
						case MediaTypes.Image:
						case MediaTypes.VideoThumb:
							this.m_pic.Visible	= true;
							this.m_host.Visible = false;
							this.m_media.Pause();

							switch (cur.Status)
							{
								case Statuses.Download:
									this.m_pic.SetDownload(cur.Progress, cur.Speed);
									break;

								case Statuses.Complete:
									this.m_pic.SetImage(cur.Image);
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

		public void ToggleZoomMode()
		{
			this.m_pic.ToggleZoomMode();
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
	}
}
