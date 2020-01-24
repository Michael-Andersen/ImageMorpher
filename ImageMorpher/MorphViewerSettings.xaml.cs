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
	/// Interaction logic for MorphViewerSettings.xaml
	/// </summary>
	public partial class MorphViewerSettings : UserControl
	{
		public MorphViewerSettings()
		{
			InitializeComponent();
		}

		private void FrameRateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			MorphViewer.FrameRate = (int)frameRateSlider.Value;
		}
	}
}
