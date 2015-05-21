using System;
using System.Windows;
using System.Windows.Controls;

namespace AZPreviewE
{
	public partial class MediaElement : UserControl
	{
		public MediaElement()
		{
			InitializeComponent();
		}

		private void Media_MediaEnded(object sender, RoutedEventArgs e)
		{
			this.Media.Position = TimeSpan.Zero;
			this.Media.Play();
		}

		private void Media_MediaOpened(object sender, RoutedEventArgs e)
		{
			this.Media.Play();
		}
	}
}
