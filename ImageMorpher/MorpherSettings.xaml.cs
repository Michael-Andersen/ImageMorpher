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
	/// Interaction logic for MorpherSettings.xaml
	/// </summary>
	public partial class MorpherSettings : UserControl
	{
		public MorpherSettings()
		{
			InitializeComponent();
		}

		private void ASlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Morpher.A_VALUE = ASlider.Value;
		}

		private void BSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Morpher.B_VALUE = BSlider.Value;
		}

		private void FramesSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Morpher.NumFrames = (int)framesSlider.Value;
		}
	}
}
