using System;
using System.Drawing;
using System.Windows.Forms;

namespace AZPreviewE
{
	[System.ComponentModel.DesignerCategory("CODE")]
	internal class PictureBoxE : PictureBox
	{
		private Image	m_image		= Properties.Resources.downloading;
		private Point	m_location	= new Point(0, 0);
		private bool	m_original	= false;

		private bool	m_move		= false;
		private Point	m_mouse;

		private bool	m_down		= true;

		public void SetImage(Image image)
		{
			if (image == null)
			{
				this.m_image	= Properties.Resources.downloading;
				this.m_down		= true;
			}
			else
			{
				this.m_image	= image;
				this.m_down		= false;
			}

			this.Refresh();
		}

		public void ImageMove(int x, int y)
		{
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
			if (this.m_location.X < 0)
				this.m_location.X = 0;
			else if (this.m_location.X > this.m_image.Width - this.Width)
					 this.m_location.X = this.m_image.Width - this.Width;

			if (this.m_location.Y < 0)
				this.m_location.Y = 0;
			else if (this.m_location.Y > this.m_image.Height - this.Height)
					 this.m_location.Y = this.m_image.Height - this.Height;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (this.m_original && !this.m_down)
			{
				int drwX, drwY, drwW, drwH;
				int imgX, imgY, imgW, imgH;

				if (this.Width >= this.m_image.Width)
				{
					drwX = (this.Width - this.m_image.Width) / 2;
					drwW = this.m_image.Width;
					imgX = 0;
					imgW = this.m_image.Width;
				}
				else
				{
					drwX = 0;
					drwW = e.ClipRectangle.Width;
					imgX = this.m_location.X;
					imgW = this.Width;
				}

				if (this.Height >= this.m_image.Height)
				{
					drwY = (this.Height - this.m_image.Height) / 2;
					drwH = this.m_image.Height;
					imgY = 0;
					imgH = this.m_image.Height;
				}
				else
				{
					drwY = 0;
					drwH = e.ClipRectangle.Height;
					imgY = this.m_location.Y;
					imgH = this.Height;
				}

				e.Graphics.DrawImage(
					this.m_image,
					new Rectangle(drwX, drwY, drwW, drwH),
					new Rectangle(imgX, imgY, imgW, imgH),
					GraphicsUnit.Pixel
					);
			}
			else
			{
				e.Graphics.DrawImage(
					this.m_image,
					this.getRectangle(e.ClipRectangle),
					new Rectangle(
						0,
						0,
						this.m_image.Width,
						this.m_image.Height),
					GraphicsUnit.Pixel
					);
			}
		}

		private Rectangle getRectangle(Rectangle e)
		{
			double scale, scaleX, scaleY;

			scaleX = (double)e.Width  / (double)this.m_image.Width;
			scaleY = (double)e.Height / (double)this.m_image.Height;

			scale = Math.Min(scaleX, scaleY);

			if (scale > 1) scale = 1;

			int w = (int)Math.Ceiling(this.m_image.Width  * scale);
			int h = (int)Math.Ceiling(this.m_image.Height * scale);
			int x = (e.Width  - w) / 2;
			int y = (e.Height - h) / 2;

			return new Rectangle(x, y, w, h);
		}

		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			if (e.Button != System.Windows.Forms.MouseButtons.Left)
				return;

			this.m_original = !this.m_original;

			this.Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button != System.Windows.Forms.MouseButtons.Left)
				return;

			this.m_move = true;
			this.m_mouse.X = e.X;
			this.m_mouse.Y = e.Y;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (!this.m_move)
				return;

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
