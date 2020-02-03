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
	/// Interaction logic for ControlLineSettings.xaml
	/// </summary>
	public partial class ControlLineSettings : UserControl
	{

		public ImageViewer DestViewer { get; set; }
		public ImageViewer SrcViewer { get;  set; }

		public ControlLineSettings()
		{
			InitializeComponent();
			lineComboBox.ItemsSource = Enum.GetValues(typeof(ControlLineVisual.Colour)).Cast<ControlLineVisual.Colour>();
			startComboBox.ItemsSource = Enum.GetValues(typeof(ControlLineVisual.Colour)).Cast<ControlLineVisual.Colour>();
			middleComboBox.ItemsSource = Enum.GetValues(typeof(ControlLineVisual.Colour)).Cast<ControlLineVisual.Colour>();
			endComboBox.ItemsSource = Enum.GetValues(typeof(ControlLineVisual.Colour)).Cast<ControlLineVisual.Colour>();
			highlightComboBox.ItemsSource = Enum.GetValues(typeof(ControlLineVisual.Colour)).Cast<ControlLineVisual.Colour>();
		}

		public void load(double lineThickness, double diameter, double tolerance, ControlLineVisual.Colour lineColour,
			ControlLineVisual.Colour start, ControlLineVisual.Colour middle, ControlLineVisual.Colour end, ControlLineVisual.Colour highlight)
		{
			lineThicknessSlider.Value = lineThickness;
			diameterSlider.Value = diameter;
			toleranceSlider.Value = tolerance;
			ControlPoint.TOLERANCE = toleranceSlider.Value;
			lineComboBox.SelectedItem = lineColour;
			startComboBox.SelectedItem = start;
			middleComboBox.SelectedItem = middle;
			endComboBox.SelectedItem = end;
			highlightComboBox.SelectedItem = highlight;
		}

		private void visualChange()
		{
			if (SrcViewer != null)
			{
				SrcViewer.updateVisualSettings();
			}
			if (DestViewer != null)
			{
				DestViewer.updateVisualSettings();
			}
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
	}
}
