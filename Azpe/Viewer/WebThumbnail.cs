using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace Azpe.Viewer
{
	internal class WebThumbnail
	{
		private string	m_url;
		private Bitmap	m_img;

		public static Image GetWebThumbnail(string url)
		{
			var thm = new WebThumbnail(url);

			return thm.m_img;
		}

		private WebThumbnail(string url)
		{
			this.m_url = url;
			Thread thd = new Thread(GetThumbnail);
			thd.SetApartmentState(ApartmentState.STA);
			thd.Start();
			thd.Join();
		}

		private void GetThumbnail()
		{
			int wm = int.MaxValue;
			int hm = int.MaxValue;

			foreach (var screen in Screen.AllScreens)
			{
				wm = Math.Min(screen.Bounds.Width,  wm);
				hm = Math.Min(screen.Bounds.Height, hm);
			}

			try
			{
				using (var web = new WebBrowser())
				{
					web.ScrollBarsEnabled		= false;
					web.ScriptErrorsSuppressed	= true;

					DateTime start = DateTime.UtcNow;
					web.Navigate(m_url);

					while ((web.ReadyState != WebBrowserReadyState.Complete && (DateTime.UtcNow -start).TotalSeconds < 10))
						Application.DoEvents();

					int w, h;

					try
					{
						dynamic doc = web.Document.DomDocument;
						dynamic body = web.Document.Body;
						body.DomElement.contentEditable = true;
						doc.documentElement.style.overflow = "hidden";
					}
					catch
					{ }

					try
					{
						w = Math.Min(web.Document.Body.ScrollRectangle.Width, wm);
						h = Math.Min(web.Document.Body.ScrollRectangle.Height, hm);
					}
					catch
					{
						w = Math.Min(web.Document.Body.ClientRectangle.Width, wm);
						h = Math.Min(web.Document.Body.ClientRectangle.Height, hm);
					}

					web.ClientSize = new Size(w, h);

					m_img = new Bitmap(w, h, PixelFormat.Format24bppRgb);
					web.DrawToBitmap(m_img, new Rectangle(0, 0, w, h));
				}
			}
			catch
			{
				if (this.m_img != null)
				{
					this.m_img.Dispose();
					this.m_img = null;
				}
			}
		}
	}
}
