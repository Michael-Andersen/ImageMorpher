using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace ImageMorpher
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		private ImageViewer destViewer;
		private ImageViewer srcViewer;
		public bool OwnerClosing { get; set; } = false;


		public SettingsWindow(ImageViewer srcViewer, ImageViewer destViewer)
		{
			InitializeComponent();
			lineComboBox.ItemsSource = Enum.GetValues(typeof(ControlLineVisual.Colour)).Cast<ControlLineVisual.Colour>();
			startComboBox.ItemsSource = Enum.GetValues(typeof(ControlLineVisual.Colour)).Cast<ControlLineVisual.Colour>();
			middleComboBox.ItemsSource = Enum.GetValues(typeof(ControlLineVisual.Colour)).Cast<ControlLineVisual.Colour>();
			endComboBox.ItemsSource = Enum.GetValues(typeof(ControlLineVisual.Colour)).Cast<ControlLineVisual.Colour>();
			highlightComboBox.ItemsSource = Enum.GetValues(typeof(ControlLineVisual.Colour)).Cast<ControlLineVisual.Colour>();
			this.srcViewer = srcViewer;
			this.destViewer = destViewer;
		}

		private void visualChange()
		{
			if (srcViewer != null)
			{
				srcViewer.updateVisualSettings();
			}
			if (destViewer != null)
			{
				destViewer.updateVisualSettings();
			}
		}

		public void load(double lineThickness, double diameter, double tolerance, ControlLineVisual.Colour lineColour,
			ControlLineVisual.Colour start, ControlLineVisual.Colour middle, ControlLineVisual.Colour end, ControlLineVisual.Colour highlight)
		{
			lineThicknessSlider.Value = lineThickness;
			diameterSlider.Value = diameter;
			toleranceSlider.Value = tolerance;
			lineComboBox.SelectedItem = lineColour;
			startComboBox.SelectedItem = start;
			middleComboBox.SelectedItem = middle;
			endComboBox.SelectedItem = end;
			highlightComboBox.SelectedItem = highlight;
		}

		private void LineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			ControlLineVisual.LINE_THICKNESS = (int)lineThicknessSlider.Value;
			visualChange();
		}

		private void DiameterSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			ControlLineVisual.DIAMETER = (int)diameterSlider.Value;
			visualChange();
		}

		private void ToleranceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			ControlPoint.TOLERANCE = toleranceSlider.Value;
		}

		private void FramesSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (srcViewer != null)
			{
				//ControlLine test = new ControlLine(srcViewer.canvas, new ControlPoint(new Point(10, 60)));
				//test.setEnd(new ControlPoint(new Point(40, 10)));
			//	double dist = ControlLine.distance(test, 60, 40);
			//	double fl = ControlLine.fracLength(test, 60, 40);
			}
			Morpher.NumFrames = (int)framesSlider.Value;
		}

		private void LineColour_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ControlLineVisual.LINE_COLOUR = (ControlLineVisual.Colour)((sender as ComboBox).SelectedItem);
			visualChange();
		}

		private void StartColour_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ControlLineVisual.START_COLOUR = (ControlLineVisual.Colour)((sender as ComboBox).SelectedItem);
			visualChange();
		}

		private void MiddleColour_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ControlLineVisual.MIDDLE_COLOUR = (ControlLineVisual.Colour)((sender as ComboBox).SelectedItem);
			visualChange();
		}

		private void EndColour_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ControlLineVisual.END_COLOUR = (ControlLineVisual.Colour)((sender as ComboBox).SelectedItem);
			visualChange();
		}

		private void HighlightColour_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ControlLineVisual.HIGHLIGHT_COLOUR = (ControlLineVisual.Colour)((sender as ComboBox).SelectedItem);
			visualChange();
		}

		private void Settings_Closing(object sender, CancelEventArgs e)
		{
			if (!OwnerClosing)
			{
				e.Cancel = true;
				Hide();
			}
		}
	}
}
