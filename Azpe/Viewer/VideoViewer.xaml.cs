using System;
using System.Windows;
using System.Windows.Controls;

namespace Azpe.Viewer
{
	public partial class VideoViewer : UserControl
	{
		public VideoViewer()
		{
			InitializeComponent();
		}

		private bool m_nowPlaying = false;

		public void Play()
		{
			this.m_nowPlaying = true;
			this.Media.Play();
		}

		public void Pause()
		{
			this.m_nowPlaying = false;
			this.Media.Pause();
		}

		private void Media_MediaEnded(object sender, RoutedEventArgs e)
		{
			this.Media.Position = TimeSpan.Zero;
			this.Play();
		}

		private void Media_MediaOpened(object sender, RoutedEventArgs e)
		{
			this.Play();
		}

		private void Media_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (this.m_nowPlaying)
				this.Pause();
			else
				this.Play();

		}
	}
}
