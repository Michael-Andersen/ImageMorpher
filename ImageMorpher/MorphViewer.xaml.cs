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
	/// Interaction logic for MorphViewer.xaml
	/// </summary>
	public partial class MorphViewer : UserControl
	{
		public MorphViewer()
		{
			InitializeComponent();
		}

		public void setImageSrc(ImageSource im)
		{
			image.Source = im;
		}

		private void playBtn_Click(object sender, RoutedEventArgs e)
		{

		}
		private void prevBtn_Click(object sender, RoutedEventArgs e)
		{

		}
		private void nextBtn_Click(object sender, RoutedEventArgs e)
		{

		}
		private void reverseBtn_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
