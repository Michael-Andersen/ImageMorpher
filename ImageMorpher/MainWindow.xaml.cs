using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageMorpher
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			destViewer.setOtherViewer(srcViewer);
			srcViewer.setOtherViewer(destViewer);
			morphViewer.Visibility = Visibility.Collapsed;
		}

		private void SetSrc_Click(object sender, RoutedEventArgs e)
		{
			srcViewer.setImage();
		}

		private void SetDest_Click(object sender, RoutedEventArgs e)
		{
			destViewer.setImage();
		}

		private void Morph_Click(object sender, RoutedEventArgs e)
		{
			srcViewer.Visibility = Visibility.Hidden;
			destViewer.Visibility = Visibility.Hidden;
			morphViewer.Visibility = Visibility.Visible;
			morphViewer.setImageSrc(srcViewer.getImage().Source);
			UpdateLayout();
		}
	}
}
