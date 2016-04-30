using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Azpe.Viewer
{
	[System.ComponentModel.DesignerCategory("CODE")]
	internal class ImageViewer : Control
	{
		private MediaTypes	m_type;
		private Statuses	m_status	= Statuses.Download;
		private Image		m_image		= null;
		private float		m_prog		= 0f;
		private float		m_speed		= 0f;

		private Point		m_location	= new Point(0, 0);
		private bool		m_original	= false;

		private bool		m_move		= false;
		private Point		m_mouse;
		
		public ImageViewer() : base()
		{
			this.DoubleBuffered = true;
		}

		public void SetDownload(float progress, float speed)
		{
			this.m_status	= Statuses.Download;
			this.m_prog		= Math.Min(progress, 1);
			this.m_speed	= speed;

			this.Invalidate();
		}
		public void SetImage(Image image, MediaTypes type)
		{
			this.m_status	= Statuses.Complete;
			this.m_image	= image;
			this.m_type		= type;

			this.CheckPosition();

			this.Invalidate();
		}
		public void SetError()
		{
			this.m_status	= Statuses.Error;

			this.Invalidate();
		}

		public void ImageMove(int x, int y)
		{
			if (this.m_status != Statuses.Complete)
				return;

			this.m_location.X += x * this.m_image.Width  / 20;
			this.m_location.Y += y * this.m_image.Height / 20;

			this.CheckPosition();

			this.Invalidate();
		}

		public void ToggleZoomMode()
		{
			this.m_original = !this.m_original;

			this.Invalidate();
		}

		protected override void OnResize(EventArgs e)
		{
			this.CheckPosition();

			this.Invalidate();
		}

		private void CheckPosition()
		{
			if (this.m_status != Statuses.Complete)
				return;

			try
			{
				if (this.m_location.X < 0)
					this.m_location.X = 0;
				else
					if (this.m_location.X > this.m_image.Width - this.Width)
						this.m_location.X = this.m_image.Width - this.Width;

				if (this.m_location.Y < 0)
					this.m_location.Y = 0;
				else
					if (this.m_location.Y > this.m_image.Height - this.Height)
						this.m_location.Y = this.m_image.Height - this.Height;
			}
			catch
			{
			}
		}
		
		private static Size			m_progSize	= new Size(120, 10);
		private static Pen			m_progLine	= Pens.DimGray;
		private static Color		m_progBack0	= Color.Gainsboro;
		private static Color		m_progBack1	= Color.LightGray;
		private static Color		m_progProg0	= Color.LightGreen;
		private static Color		m_progProg1	= Color.Green;
		private static Font			m_strFont	= new Font("Consolas", 7.0f, FontStyle.Regular);
		private static float		m_dia		= 8;
		private static StringFormat m_strPers	= new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Far };
		private static StringFormat m_strSpeed	= new StringFormat() { Alignment = StringAlignment.Far,  LineAlignment = StringAlignment.Far };

		private static GraphicsPath DrawArc(RectangleF rect)
		{
			GraphicsPath path = new GraphicsPath();

			//시계방향
			path.AddArc(new RectangleF(rect.X		- m_dia / 2, rect.Y,				m_dia, m_dia), 180, 90);
			path.AddArc(new RectangleF(rect.Right	+ m_dia / 2, rect.Y,				m_dia, m_dia), 270, 90);
			path.AddArc(new RectangleF(rect.Right	+ m_dia / 2, rect.Bottom - m_dia,	m_dia, m_dia), 0, 90);
			path.AddArc(new RectangleF(rect.X		- m_dia / 2, rect.Bottom - m_dia,	m_dia, m_dia), 90, 90);

			path.CloseFigure();

			return path;
		}

		private static string GetSpeed(float down)
		{
			if (down < 1000)
				return string.Format("{0:##0.0} B/s", down);
			else if (down < 1024 * 1000)
				return string.Format("{0:##0.0} KB/s", down / 1024);
			else
				return string.Format("{0:##0.0} MB/s", down / 1024 / 1024);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.Clear(this.BackColor);

			if (this.m_status == Statuses.Download)
			{
				e.Graphics.SmoothingMode		= SmoothingMode.AntiAlias;
				e.Graphics.InterpolationMode	= InterpolationMode.HighQualityBicubic;

				var back = new RectangleF(
					(this.Width  - ImageViewer.m_progSize.Width)  / 2,
					(this.Height - ImageViewer.m_progSize.Height) / 2,
					ImageViewer.m_progSize.Width,
					ImageViewer.m_progSize.Height);

				var prog = new RectangleF(
					(this.Width  - ImageViewer.m_progSize.Width)  / 2,
					(this.Height - ImageViewer.m_progSize.Height) / 2,
					ImageViewer.m_progSize.Width * this.m_prog,
					ImageViewer.m_progSize.Height);

				using (var pathBack  = DrawArc(back))
				using (var pathProg  = DrawArc(prog))
				using (var brushBack = new LinearGradientBrush(back, ImageViewer.m_progBack0, ImageViewer.m_progBack1, LinearGradientMode.Vertical))
				using (var brushProg = new LinearGradientBrush(back, ImageViewer.m_progProg0, ImageViewer.m_progProg1, LinearGradientMode.Vertical))
				{
					e.Graphics.FillPath(brushBack, pathBack);
					e.Graphics.FillPath(brushProg, pathProg);
					e.Graphics.DrawPath(ImageViewer.m_progLine, pathBack);
				}

				e.Graphics.DrawString(
					string.Format("{0:##0}%", this.m_prog * 100),
					ImageViewer.m_strFont,
					Brushes.Black,
					new RectangleF(back.X, back.Y - 20, back.Width, 20),
					ImageViewer.m_strPers);

				if (this.m_speed != 0)
					e.Graphics.DrawString(
						ImageViewer.GetSpeed(this.m_speed),
						ImageViewer.m_strFont,
						Brushes.Black,
						new RectangleF(back.X, back.Y - 20, back.Width, 20),
						ImageViewer.m_strSpeed);
			}
			else
			{
				Rectangle dest, src;

				if (this.m_status == Statuses.Complete)
				{
					this.getRectangle(this.m_image, this.m_original, out dest, out src);
					e.Graphics.DrawImage(this.m_image, dest, src, GraphicsUnit.Pixel);

                    if (this.m_type == MediaTypes.PageThumb ||
                        this.m_type == MediaTypes.VideoThumb)
                        e.Graphics.DrawImage(Properties.Resources.browser, 0, 0);
				}
				else
				{
					this.getRectangle(Properties.Resources.error, false, out dest, out src);
					e.Graphics.DrawImage(Properties.Resources.error, dest, src, GraphicsUnit.Pixel);

                    if (this.m_type == MediaTypes.PageThumb ||
                        this.m_type == MediaTypes.VideoThumb)
                        e.Graphics.DrawImage(Properties.Resources.browser, dest.Left, dest.Top);
				}
			}
		}

		private void getRectangle(Image image, bool orignalSize, out Rectangle destRect, out Rectangle srcRect)
		{
			if (orignalSize)
			{
				int drwX, drwY, drwW, drwH;
				int imgX, imgY, imgW, imgH;

				if (this.Width >= image.Width)
				{
					drwX = (this.Width - image.Width) / 2;
					drwW = image.Width;
					imgX = 0;
					imgW = image.Width;
				}
				else
				{
					drwX = 0;
					drwW = this.Width;
					imgX = this.m_location.X;
					imgW = this.Width;
				}

				if (this.Height >= image.Height)
				{
					drwY = (this.Height - image.Height) / 2;
					drwH = image.Height;
					imgY = 0;
					imgH = image.Height;
				}
				else
				{
					drwY = 0;
					drwH = this.Height;
					imgY = this.m_location.Y;
					imgH = this.Height;
				}

				destRect = new Rectangle(drwX, drwY, drwW, drwH);
				srcRect  = new Rectangle(imgX, imgY, imgW, imgH);
			}
			else
			{
				double scaleX = this.Width  / (double)image.Width;
				double scaleY = this.Height / (double)image.Height;
				double scale  = Math.Min(scaleX, scaleY);

				if (scale > 1) scale = 1;

				int w = (int)Math.Ceiling(image.Width  * scale);
				int h = (int)Math.Ceiling(image.Height * scale);
				int x = (this.Width  - w) / 2;
				int y = (this.Height - h) / 2;

				destRect = new Rectangle(x, y, w, h);
				srcRect  = new Rectangle(0, 0, image.Width, image.Height);
			}
		}

		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			if (e.Button != System.Windows.Forms.MouseButtons.Left ||
				this.m_status != Statuses.Complete)
				return;

			this.m_original = !this.m_original;

			this.Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button != System.Windows.Forms.MouseButtons.Left ||
				this.m_status != Statuses.Complete)
				return;

			this.m_move = true;
			this.m_mouse.X = e.X;
			this.m_mouse.Y = e.Y;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (this.m_status != Statuses.Complete || !this.m_move) return;

			this.m_location.X += (int)((this.m_mouse.X - e.X) * 1.0d * this.m_image.Width / this.Width);
			this.m_location.Y += (int)((this.m_mouse.Y - e.Y) * 1.0d * this.m_image.Height / this.Height);

			this.m_mouse.X = e.X;
			this.m_mouse.Y = e.Y;

			this.CheckPosition();

			this.Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			this.m_move = false;

			this.Invalidate();
		}
	}
}
